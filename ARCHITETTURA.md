# Architettura del plugin

Stato: **funzionante**, verificato in gioco il 16 agosto 2026. Versione `0.1.0`.

Il plugin aggiunge l'italiano al selettore lingue e serve il testo tradotto mentre
il gioco lo chiede. **Non riscrive i dati del gioco e non tocca nessun file nella
cartella di installazione.**

## Il principio: servire in lettura, non scrivere

La strada intuitiva — riversare le traduzioni nella tabella `I2Languages` — è stata
provata e **scartata**: scrivere ~22.000 stringhe negli array Il2Cpp fa crashare il
processo a livello nativo, in modo riproducibile. Con 11.000 righe di traduzione
finale ci saremmo arrivati comunque.

Il plugin tiene quindi le traduzioni in un `Dictionary` suo e risponde nei postfix
Harmony delle letture. Vantaggi collaterali non da poco:

- il file di traduzione si rilegge a ogni avvio, **senza ricompilare**;
- i dati del gioco restano intatti, quindi niente effetti collaterali;
- una traduzione mancante ripiega sull'inglese senza celle nulle.

L'unica modifica ai dati del gioco è l'aggiunta della lingua, indispensabile perché
la voce compaia nel selettore.

## I file

| File | Ruolo |
|---|---|
| `Plugin.cs` | avvio: configurazione, caricamento, applicazione delle patch |
| `TranslationStore.cs` | legge `italian.json` da disco a ogni avvio |
| `LanguageRegistration.cs` | aggiunge "Italiano" a tutte le sorgenti |
| `Patches.cs` | gli agganci Harmony |
| `LanguageMemory.cs` | ricorda la lingua, che il gioco non sa salvare |

## Gli agganci, e perché ciascuno esiste

| Aggancio | Perché |
|---|---|
| `I2LocalizationDatabase.CreateLanguagesData` (prefix) | registra l'italiano **prima** che la lista lingue venga costruita e messa in cache |
| `LocalizationSystem.Initialize` (postfix) | primo punto in cui il sistema è utilizzabile. `InitializeLanguages`, malgrado il nome, non viene **mai** chiamato all'avvio |
| `I2LocalizationDatabase.GetValue` (postfix) | percorso di lettura del wrapper del gioco: serve la traduzione, altrimenti ripiega sull'inglese |
| `TermData.GetTranslation` (postfix) | **secondo** percorso di lettura, per l'interfaccia che usa I2 direttamente |
| `LocalizationSystem.SetCurrentLanguage` (postfix) | annota la lingua scelta |
| `OptionSetting.GetSettingValueFrom` (postfix) | ripristina la lingua all'avvio — è la strada che il gioco percorre davvero |
| `LanguageSetting.GetDefaultValue` (postfix) | ripristino su un profilo che non ha mai salvato una lingua |
| `AutoSkipController.ResetTime` (finalizer) | rete diagnostica: se quel metodo torna a sollevare eccezioni, vogliamo la chiave nel log |

### I due percorsi di lettura

Non è un dettaglio: **l'interfaccia usa entrambi contemporaneamente.** Nella
schermata Impostazioni, "Lingua" arriva dal wrapper del gioco mentre
`UI/Settings/Video/Title` arriva da I2 diretto. Agganciando solo il primo, metà
schermata mostrava le **chiavi grezze** al posto del testo.

`TermData` porta con sé la propria chiave nel campo `Term`, quindi la stessa
ricerca funziona su entrambi i percorsi.

### Il ripiego sull'inglese non è cosmetico

`AddLanguage` allarga l'array di traduzioni di ogni termine riempiendolo di `null`.
`GetValue` restituisce quel `null` tale e quale, e
`AutoSkipController.ResetTime()` — che misura la lunghezza della battuta per
decidere quanto tenerla a schermo, e gira **anche ad auto-advance spento** —
solleva `NullReferenceException`. Il dialogo si blocca alla prima riga non
tradotta. Senza ripiego, una traduzione parziale è ingiocabile.

### La persistenza della lingua

Il gioco non salva più **nessuna** impostazione: `OptionSetting.GetSaveData()`
solleva un'eccezione che fa fallire l'intero `SettingsSystem.Save()`. Succede sul
gioco non modificato, cambiando qualunque opzione.

Il *caricamento* però funziona, e il file esistente contiene `LanguageSetting →
"en"`. Quindi non basta fornire un default: **il valore salvato stantio vince**, e
`GetDefaultValue()` non viene mai nemmeno chiamato. Il ripristino va fatto in
`GetSettingValueFrom`, dove il valore salvato viene interpretato.

Tentativi che **non** funzionano, per non ripeterli:

- chiamare `SetCurrentLanguage` in `LocalizationSystem.Initialize`: il gioco
  applica il proprio valore subito dopo, e — passando dal nostro postfix —
  **cancella pure la memoria**, riportandola a `en`;
- agganciare solo `GetDefaultValue`: mai invocato finché esiste un file di
  impostazioni.

## Il file di traduzione

`translations/italian.json`, installato accanto al DLL. Oggetto JSON piatto,
chiave del termine → testo italiano:

```json
{
  "UI/MainMenu/Buttons/NewGame": "Nuova partita",
  "Dialogue/R_NIGHT_1/LINE-6": "Fammi capire bene: hai perso il tuo <link=\"Sire\">sire</link>, eh?"
}
```

JSON e non CSV perché il testo contiene a piene mani virgolette, virgole e a capo:
il quoting CSV sarebbe una fonte continua di errori silenziosi.

Un valore vuoto significa "non ancora tradotto" e lascia la riga in inglese. Un
file mancante o malformato lascia il gioco interamente in inglese invece di
romperlo.

## Configurazione

In `BepInEx/config/dev.xodryx.rony.italian.cfg`:

- `Enabled` — a `false` il gioco gira **intatto** tenendo accesa la diagnostica.
  È servito due volte per distinguere i nostri difetti da quelli del gioco, ed è
  la prima cosa da usare davanti a un errore.
- `LastLanguage` — la lingua da ripristinare all'avvio.

## Installazione

    bash tools/deploy.sh

Compila, installa e **verifica gli md5**, interrompendosi se non coincidono. Si
rifiuta di partire a gioco aperto. Non è pignoleria: una build vecchia rimasta in
`BepInEx/plugins/` è costata tre esecuzioni di conclusioni sbagliate.

## Cosa manca

1. Il grosso della traduzione: 31 stringhe su 11.141. Il flusso di lavoro è pronto
   e descritto in `FLUSSO.md`.
2. Un controllo CI che esegua `tools/apply.py --check` a ogni push, così un
   marcatore rotto non entra nel repo.
3. Una pipeline di release che costruisca lo zip al push di un tag `v*`, come su
   *Shadows of New York*. Due differenze da tenere presenti:
   - **BepInEx 6 IL2CPP pesa ~34 MB**, contro i ~640 KB della build Mono di
     *Shadows*. E il primo avvio impiega una trentina di secondi a generare gli
     assembly interop, con la finestra apparentemente ferma: va scritto nelle
     istruzioni, o sembrerà bloccato.
   - **Da verificare se possiamo redistribuire BepInEx.** In caso contrario lo zip
     conterrà solo `RonyItalian.dll` e `italian.json`, e l'utente dovrà installare
     BepInEx da sé — più scomodo, ma è il vincolo.
