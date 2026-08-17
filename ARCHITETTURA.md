# Architettura del plugin

Stato: **funzionante**, verificato in gioco il 16 agosto 2026. Versione `0.1.0`.

Il plugin aggiunge l'italiano al selettore lingue e serve il testo tradotto mentre
il gioco lo chiede. **Non riscrive la tabella delle traduzioni e non tocca nessun file
nella cartella di installazione.**

## Il principio: servire in lettura, non scrivere

La strada intuitiva — riversare le traduzioni nella tabella `I2Languages` — è stata
scartata. La prima prova, che scriveva direttamente negli array Il2Cpp, faceva
crashare il processo a livello nativo in modo riproducibile. *(Nota emersa dopo:
passando invece per `TermData.SetTranslation` la scrittura funziona — 11.152 celle per
sorgente, nessun crash. Non serve comunque, ed è codice rischioso senza un beneficio
dimostrato.)*

Il plugin tiene quindi le traduzioni in un `Dictionary` suo e risponde nei postfix
Harmony delle letture. Vantaggi collaterali non da poco:

- il file di traduzione si rilegge a ogni avvio, **senza ricompilare**;
- i dati del gioco restano intatti, quindi niente effetti collaterali;
- una traduzione mancante ripiega sull'inglese senza celle nulle.

Le eccezioni al principio sono tre, tutte inevitabili e circoscritte:

1. **L'aggiunta della lingua**, indispensabile perché la voce compaia nel selettore.
2. **Le due descrizioni della selezione del personaggio** (`CharacterPanelText.cs`):
   quelle etichette non hanno nessun componente di localizzazione e nessuno ci scrive,
   quindi non c'è nessuna lettura da intercettare.
3. **Le stringhe che esistono solo dentro un prefab** (`BakedText.cs`): non stanno nella
   tabella dei termini, quindi non c'è nemmeno una chiave da tradurre. Si riconoscono
   letteralmente dall'inglese e si sostituiscono. Per ora è una sola.

Le ultime due scrivono nella scena invece di rispondere a una lettura, hanno ciascuna
il proprio interruttore (`FixCharacterPanel`, `FixBakedText`) e non toccano comunque
nessun dato di localizzazione. Il perché sta in `STATO.md`.

## I file

| File | Ruolo |
|---|---|
| `Plugin.cs` | avvio: configurazione, caricamento, applicazione delle patch |
| `TranslationStore.cs` | legge `italian.json` da disco a ogni avvio |
| `LanguageRegistration.cs` | aggiunge "Italiano" a tutte le sorgenti |
| `Patches.cs` | gli agganci Harmony |
| `LanguageMemory.cs` | ricorda la lingua, che il gioco non sa salvare |
| `CurrentLanguage.cs` | la lingua in corso, senza interrogare il gioco a ogni battuta |
| `SceneTranslation.cs` | risposta comune ai percorsi di lettura di I2 |
| `CharacterPanelText.cs` | riempie le due etichette che il gioco non scrive mai |
| `BakedText.cs` | sostituisce le stringhe che vivono solo dentro un prefab |
| `UntranslatedReport.cs` | il setaccio che le trova: si accende dal config |
| `Diagnostics.cs` | le poche righe di log che valgono il loro rumore |

## Gli agganci, e perché ciascuno esiste

| Aggancio | Perché |
|---|---|
| `I2LocalizationDatabase.CreateLanguagesData` (prefix) | registra l'italiano **prima** che la lista lingue venga costruita e messa in cache |
| `LocalizationSystem.Initialize` (postfix) | primo punto in cui il sistema è utilizzabile. `InitializeLanguages`, malgrado il nome, non viene **mai** chiamato all'avvio |
| `I2LocalizationDatabase.GetValue(chiave, lingua)` (postfix) | percorso di lettura del wrapper del gioco: serve la traduzione, altrimenti ripiega sull'inglese |
| `I2LocalizationDatabase.GetValue(chiave, lingua, parametri)` (postfix) | **la stessa lettura con i parametri, ed è un metodo a sé**: non passa dalla prima. Vedi sotto |
| `TermData.GetTranslation` (postfix) | **secondo** percorso di lettura, per l'interfaccia che usa I2 direttamente |
| `LanguageSourceData.TryGetTranslation` (postfix) | **terzo** percorso, quello dei componenti `Localize` in scena. Consegna per riferimento: vedi sotto |
| `LocalizationManager.GetTranslation` / `GetTermTranslation` (postfix) | lo stesso terzo percorso preso in cima, dove la risposta torna **per valore** ed è quindi l'unica che arriva davvero |
| `Localize.OnLocalize` (postfix) | dopo che I2 ha finito con un componente: è il momento in cui riempire le etichette che nessuno scrive |
| `LocalizationSystem.SetCurrentLanguage` (postfix) | annota la lingua scelta |
| `OptionSetting.GetSettingValueFrom` (postfix) | ripristina la lingua all'avvio — è la strada che il gioco percorre davvero |
| `LanguageSetting.GetDefaultValue` (postfix) | ripristino su un profilo che non ha mai salvato una lingua |
| `AutoSkipController.ResetTime` (finalizer) | rete diagnostica: se quel metodo torna a sollevare eccezioni, vogliamo la chiave nel log |

### I quattro percorsi di lettura

Non è un dettaglio: **l'interfaccia li usa contemporaneamente.** Nella schermata
Impostazioni, "Lingua" arriva dal wrapper del gioco mentre `UI/Settings/Video/Title`
arriva da I2 diretto. Agganciando solo il primo, metà schermata mostrava le **chiavi
grezze** al posto del testo.

Il terzo è quello dei componenti `I2.Loc.Localize` appoggiati direttamente agli
oggetti della scena: non passa né dal wrapper del gioco né da `TermData`, ma da
`LocalizationManager.GetTranslation` → `TryGetTranslation` → quella della sorgente.

Il quarto **non è un percorso diverso ma la stessa `GetValue` con un'altra firma**,
quella che accetta un `ParameterGetter`. Non chiama la prima: è un metodo indipendente.
Averla trascurata lasciava scoperta un'intera strada, e nel modo peggiore — il termine
non risultava né servito né mancante, perché non arrivava affatto. A schermo: nelle
impostazioni video la modalità schermo era una riga vuota, mentre l'inglese leggeva la
propria cella e mostrava *Windowed*.

**Regola generale**: prima di concludere che un testo non passa da nessuna traduzione,
controllare che il metodo non abbia altre firme. `grep -a -o -E "NativeMethodInfoPtr_NOME[A-Za-z0-9_]*"`
sull'assembly interop le elenca tutte.

`TermData` porta con sé la propria chiave nel campo `Term`, quindi la stessa
ricerca funziona su tutti i percorsi.

### Una postfix deve scrivere nel valore di ritorno

Costata mezza giornata, quindi vale scriverla. `LanguageSourceData.TryGetTranslation`
consegna la traduzione in un **parametro per riferimento**. Una postfix Harmony che
scrive lì dentro non arriva al chiamante nativo: il log annuncia la consegna, e a
schermo non cambia niente. Tutte le patch che funzionano scrivono in `__result`.
Per questo il terzo percorso è agganciato anche in cima, su `GetTranslation`, che
restituisce una stringa.

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

## Il controllo automatico

`.github/workflows/check.yml` esegue `tools/apply.py --check` a ogni push, e poi
rigenera `translations/italian.json` verificando con `git diff --exit-code` che il
file committato sia davvero quello che i blocchi producono. Il secondo passo serve
quanto il primo: i blocchi sono la fonte di verità, ma è il file generato che finisce
in gioco, e senza controllo può restare indietro senza che nessuno se ne accorga.

**Il problema era che `apply.py` aveva bisogno di `dump/`**, che non è versionato
perché contiene il testo inglese del gioco, protetto da copyright. Un runner GitHub
non ce l'ha e non può averlo. La soluzione è
`reference/english_fingerprints.json`: per ogni chiave l'*impronta* della riga
inglese — nomi dei tag, nomi dei segnaposto, numero di gruppi fra parentesi, a capo,
lunghezza. È struttura, non prosa: 75 tag distinti e una manciata di numeri, da cui
nessuna frase è ricostruibile. `apply.py` la rigenera dal dump a ogni scrittura,
quindi non può andare fuori sincrono, e la usa al posto del dump quando il dump non
c'è.

## La release

`tools/release.sh` costruisce lo zip. **Gira in locale e non può essere spostato in
CI**: il plugin compila contro i ~152 assembly interop che BepInEx genera dai
metadati IL2CPP del gioco al primo avvio. Quegli assembly derivano da una copia del
gioco, quindi nessun runner ospitato può produrre questa DLL. Il tag `v*` può
attivare la creazione della release su GitHub, ma il file va costruito e caricato
da chi ha il gioco.

**Sì, possiamo redistribuire BepInEx**: è LGPL-2.1, che consente di ridistribuire i
binari non modificati a patto che il testo della licenza li accompagni e la fonte
sia dichiarata. Lo script lo fa con `--with-bepinex <zip>`: scompatta l'archivio
ufficiale, scrive `BEPINEX.txt` con attribuzione e link al sorgente, e **si rifiuta
di impacchettare se nell'archivio non trova il testo della licenza**, così una
release non può violare la LGPL per distrazione. Senza quell'opzione lo zip contiene
solo `RonyItalian.dll` e `italian.json`, e BepInEx se lo installa l'utente.

Due cose da ricordare, che valgono comunque:

- **BepInEx 6 IL2CPP pesa ~34 MB**, contro i ~640 KB della build Mono di *Shadows
  of New York*.
- **Il primo avvio impiega una trentina di secondi** a generare gli assembly
  interop, con la finestra apparentemente ferma. È scritto nel `LEGGIMI.txt` che
  lo script mette nello zip, altrimenti sembrerà bloccato.

## Cosa manca

Solo le 747 battute del demo di Cracovia, che non sono di questo gioco: vedi
`STATO.md`.
