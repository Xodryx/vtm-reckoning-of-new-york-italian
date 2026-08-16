# Ricognizione tecnica — 16 agosto 2026

Risposta ai punti 1–3 di `START-HERE.md`. Tutto verificato sui file installati
della build del 29 dicembre 2025.

---

## 1. Il numero che mancava

**11.141 stringhe inglesi da tradurre, 932.478 caratteri, ~168.000 parole.**

La tabella `I2Languages` contiene 11.152 termini, tutti di tipo `Text`; 11.141
hanno testo inglese non vuoto. Il francese ne ha 10.312, quindi anche la
localizzazione ufficiale ha dei buchi.

Per confronto: *Shadows of New York* erano 5.699 battute. **Questo è circa il
doppio delle voci**, e con battute mediamente più lunghe.

Ripartizione per prefisso della chiave:

| Categoria | Termini | Caratteri EN |
|---|---:|---:|
| `Dialogue/…` | 8.905 | 783.891 |
| `CardinalTMP/…` | 714 | 51.149 |
| `UI/…` | 510 | 10.746 |
| `InnerVoices/…` | 291 | 16.434 |
| `Quest/…` | 201 | 13.223 |
| `Glossary/…` | 172 | 23.488 |
| `Journal/…` | 168 | 30.003 |
| `ActorsDatabase/…` | 87 | 828 |
| `Achievements/…` | 49 | 1.225 |
| altro (`VariaCRD`, `TextPanels`, `ActorsCRD`) | 54 | 1.466 |

Lunghezza delle battute: mediana 66 caratteri, 90° percentile 178, 99° 296,
massimo 883.

Marcatori da preservare nel testo tradotto:

- **tag TMP in 1.015 stringhe** — `<i>`, `<b>`, `<gradient="VoiceColorGrad">`,
  e soprattutto `<link="Ravnos">` che aggancia le voci del glossario. È
  l'equivalente dei marcatori `[ID;termine]` di *Shadows*: l'attributo va
  lasciato identico, si traduce solo il testo dentro.
- **nomi di poteri fra parentesi quadre in 302 stringhe** — `[DAUNT]`,
  `[CLOAK OF SHADOWS]`, `[FERAL WHISPERS]`, `[CHIMERSTRY]`. Terminologia VtM:
  vanno risolti col glossario ufficiale, non tradotti a orecchio.
- **segnaposto `{[…]}` in 12 stringhe** — `{[DialogueSkip]}`, `{[controller]}`,
  `{[button]}`, `{[CHARACTER_STYLE]}`. Da lasciare intatti.
- `\n` esplicito in 79 stringhe.

Estratto con `tools/extract_i2.py`, che rigenera `dump/i2_terms.csv` e
`dump/i2_terms.json`. **Quella cartella resta fuori dal versionamento**: è il
testo inglese del gioco.

## 2. Dove sta la tabella (non dove pensavamo)

`I2Languages` **non è negli Addressables**. Il catalogo la registra con il
`LegacyResourcesProvider`, cioè viene caricata da `Resources`, e nelle build
IL2CPP quella roba finisce dentro `data.unity3d`. È lì, 2,8 MB.

Verificabile con `tools/decode_catalog.py`, che decodifica il catalogo
Addressables: l'unica voce di tipo `I2.Loc.LanguageSourceAsset` è
`I2Languages`, provider `LegacyResourcesProvider`.

Conseguenza pratica: **i 22 bundle da 425 MB non ci servono per il testo.** Un
file solo, e per giunta non un bundle.

Il gioco è IL2CPP, quindi l'asset non porta con sé il type tree e UnityPy non lo
sa deserializzare. `extract_i2.py` percorre a mano il formato binario di Unity,
usando il layout dei campi letto dal dump di `GameAssembly.dll`. Il parser
verifica di finire esattamente in fondo al buffer, quindi se il gioco viene
aggiornato e il layout cambia, fallisce invece di restituire spazzatura.

## 3. La prima incognita è sciolta: l'italiano si può aggiungere

**L'enum delle lingue del gioco contiene già l'italiano.** Da
`DrawDistance.Localization`:

```csharp
public enum Language {
    de = 0, en = 1, es = 2, fr = 3, it = 4, ja = 5,
    ko = 6, pl = 7, pt_BR = 8, ru = 9, zh_CN = 10, zh_TW = 11
}
```

Ed esiste già anche l'etichetta tradotta per il selettore:
`UI/Settings/Language_IT` → `"Italian"`. Insieme alle altre dieci
(`Language_ENG`, `Language_PL`, `Language_DE`, …): **è questa la "tabella di
lingue" che `START-HERE.md` aveva intravisto in `data.unity3d`**, non un enum
del codice. Sono etichette preparate per localizzazioni mai fatte.

Ma `I2Languages` ha **solo due colonne, `English [en]` e `French [fr]`**. È
questa la lista che comanda.

La catena che riempie il selettore, ricostruita dal dump:

```
LanguageSetting.CreateOptions()            (DrawDistance.Settings)
  └─ LocalizationSystem.GetLanguagesData()
       └─ I2LocalizationDatabase.GetLanguagesData()
            └─ CreateLanguagesData()
                 ├─ per ogni lingua di I2: GetLanguageFromCode(code) → Language
                 └─ filtro IsAllowedLanguage(Language)
```

Quindi **la voce "Italiano" compare nel selettore se aggiungiamo una lingua di
codice `it` alla sorgente I2 prima che quella lista venga costruita.** Nessun
enum da forzare, nessuna patch agli asset: è più semplice del giro fatto su
Pixel Crushers per *Shadows*.

E per iniettare il testo l'interfaccia è già pubblica:

```csharp
public interface ILocalizationDatabase {
    void Add(string key, string value, Language language);
    void SetValue(string key, string value, Language language);
    string GetValue(string key, Language language);
    List<LanguageData> GetLanguagesData();
    void SetCurrentLanguage(Language language);
    List<LocalizationData> AllowedLanguages { get; set; }
}
```

`LocalizationSystem.Instance` è statico, e `AllowedLanguages` ha il setter
pubblico. Il plugin ha tutti gli agganci che gli servono senza toccare un file
del gioco.

## 4. Rischi: due scesi, uno nuovo, uno da verificare

**Sceso — sincronizzazione Google.** La sorgente I2 punta a un foglio Google
("I2Loc VtM RONY Localization", ultimo aggiornamento 4 settembre 2024) con tanto
di URL del web service. Sarebbe stato un guaio: una sincronizzazione all'avvio
avrebbe sovrascritto le nostre stringhe. Ma `GoogleUpdateFrequency = Never`.
Non scarica mai a runtime.

**Sceso — i caratteri accentati.** Il francese è già una lingua completa del
gioco, quindi i font hanno di sicuro `à è é ù ç`. All'italiano mancherebbe solo
`ì` e `ò`, che stanno nello stesso blocco Latin-1. Rischio font quasi nullo,
ma da confermare a schermo.

**Sceso — il filtro `IsAllowedLanguage`.** Verificato a runtime: `AllowedLanguages`
ha **zero gruppi** e `LocalizationFilterId = 0`. Non filtra niente, e l'italiano
passa senza doverlo toccare.

**Sceso — BepInEx 6 IL2CPP.** Era il rischio grosso, ed è caduto: la build BE
**#785** (28 giugno 2026, `BepInEx-Unity.IL2CPP-win-x64`) carica senza un errore.
Genera 155 assembly interop in **circa 30 secondi**, non minuti, e `ErrorLog.log`
resta vuoto. Un plugin compilato con .NET 6 si carica e le patch Harmony si
applicano sui metodi IL2CPP.

**Ridimensionato — il doppiaggio.** Resta vero che i limiti di lunghezza vanno
stretti, ma ora abbiamo i numeri su cui tararli (mediana 66, 99° percentile 296).

**Nuovo, ma non nostro — il salvataggio delle impostazioni è rotto nel gioco
originale.** Cambiando una qualunque opzione il gioco solleva:

```
ArgumentOutOfRangeException: Index was out of range.
  at System.Collections.Generic.List`1[T].get_Item (Int32 index)
  at DrawDistance.Settings.OptionSetting.GetSaveData ()
  at DrawDistance.Settings.SettingsCategory.GetSaveData ()
  at DrawDistance.Settings.SettingsConfig.GetSaveData ()
  at DrawDistance.Settings.SettingsSystem.Save ()
  at DrawDistance.Ui.DdOptionSwitcher.OnPointerClickNext (...)
```

Verificato col plugin in modalità osservazione (`InjectItalian = false`), cioè
sul gioco **non modificato**, passando da inglese a francese: succede lo stesso.
Nel log di una singola sessione compare 11 volte. Non è causato da noi e non è
causato da BepInEx.

Conta però per un motivo, e il motivo si è avverato. `SettingsConfig.GetSaveData()`
percorre *tutte* le impostazioni, quindi una sola con l'indice fuori intervallo fa
fallire il salvataggio intero. **Nessuna impostazione viene più salvata.**

Verificato in due modi:

- il file delle impostazioni,
  `%USERPROFILE%\AppData\LocalLow\DrawDistance\VtM Reckoning of New York\Steam\<steamid>\Fullgame\Common\Settings`,
  **non viene riscritto** — risaliva a gennaio 2026 dopo una sessione intera di
  cambi lingua, e conteneva ancora `LanguageSetting → "en"`;
- al riavvio, dopo aver scelto l'italiano ed essere usciti dal menu, il plugin
  registra `CurrentLanguage at startup = en`.

**Conseguenza per noi: il plugin deve ricordarsi da sé la lingua** (una voce nella
config di BepInEx) e riapplicarla all'avvio con
`LocalizationSystem.SetCurrentLanguage(Language.it)`. Poche righe, ma vanno
previste fin dall'architettura.

Nota di formato utile: nel file, la lingua è salvata come **chiave testuale**
(`"Value":"en"`), non come indice numerico. Se un giorno il salvataggio del gioco
venisse riparato, `"it"` ci entrerebbe senza attriti.

## 5. Verifica a runtime: l'italiano funziona

Provato sulla copia di lavoro in `C:\Users\Rodrigo\Documents\RoNY-game-copy\`,
con BepInEx 6 e il plugin sonda in `plugin/`. Risultato: **"Italiano" compare nel
selettore lingue e le stringhe iniettate si vedono a schermo.**

La trappola che è costata tre giri, e che va ricordata perché non è deducibile
dal dump:

> Le sorgenti di lingua sono **due oggetti distinti**, non uno.
> `I2LocalizationDatabase._databaseAsset.mSource` e
> `LocalizationManager.Sources[0]` hanno lo stesso contenuto — stessi 11.152
> termini — ma puntatori diversi (`0x…9A0` contro `0x…8F0`).

Scrivere su una sola delle due non dà errore, semplicemente non fa niente di
visibile; e aggiungere la lingua a una sola fa poi indicizzare a I2 una lista di
lingue più corta, con eccezione a runtime. **Vanno aggiornate tutte e due.**

Sequenza che funziona, dal plugin:

1. Postfix su `LocalizationSystem.Initialize` (è l'aggancio che scatta davvero
   all'avvio: `InitializeLanguages` **non** viene mai chiamato).
2. Raccogliere le sorgenti distinte, deduplicate per `Pointer`.
3. Su ognuna: `OnMissingTranslation = Fallback`, poi
   `AddLanguage("Italiano", "it")` se manca, poi `UpdateDictionary(true)`.
   `AddLanguage` allarga da sé l'array di traduzioni di tutti gli 11.152 termini
   (verificato: `term array widths = 3`).
4. Iniettare con `TermData.SetTranslation(indiceItaliano, testo, null)`.
5. `LocalizationManager.LocalizeAll(true)`.

Dopodiché `GetLanguagesData()` restituisce tre lingue, con
`[2] Name=Italiano Code=it Language=it`: `GetLanguageFromCode("it")` mappa
correttamente sull'enum, e il filtro lascia passare.

La seconda trappola, peggiore della prima perché non si manifesta subito:

> `AddLanguage` allarga l'array di traduzioni di ogni termine riempiendolo di
> **`null`**, non di stringhe vuote. Verificato: `11152 were null`, su entrambe le
> sorgenti.

`I2LocalizationDatabase.GetValue()` restituisce quel `null` così com'è, senza
passare dal meccanismo `OnMissingTranslation` di I2 — quindi impostare `Fallback`
**non serve a niente**. Il risultato non è una riga vuota a schermo, è molto
peggio: `AutoSkipController.ResetTime()` misura la **lunghezza del testo** per
decidere quanto tenere la battuta a schermo, incontra il `null` e solleva
`NullReferenceException`. Il dialogo si blocca, il pulsante *Continua* smette di
rispondere.

Succede alla **prima riga non tradotta**, quindi con una traduzione parziale il
gioco si pianta quasi subito. Non si aggira disattivando l'auto-advance:
`OnCreateDialogueLines` è agganciato in `BindToEvents()` e gira comunque. E non è
un difetto del gioco: la stessa partita, avviata con `InjectItalian = false`,
prosegue senza un errore.

**Rimedio sbagliato, provato e scartato:** riempire in anticipo tutte le 11.152
celle italiane con il testo inglese. Funziona sulla carta, ma scrivere quella
quantità di stringhe negli array Il2Cpp fa **crashare il gioco a livello nativo**
pochi istanti dopo, in modo riproducibile: nessuna eccezione gestita, `Player.log`
troncato di netto.

**Rimedio giusto:** ripiegare sull'inglese **al momento della lettura**, con un
postfix Harmony su `I2LocalizationDatabase.GetValue(string, Language)`:

```csharp
if (string.IsNullOrEmpty(__result) && language != Language.en)
    __result = __instance.GetValue(key, Language.en) ?? string.Empty;
```

Costa una comparazione per lettura e non tocca la memoria del gioco. Verificato:
il dialogo prosegue oltre le righe non tradotte, che restano in inglese, con zero
errori nel log. **Attenzione:** lo stesso postfix su `TermData.GetTranslation` da
solo **non basta** — quel percorso non viene attraversato da `GetValue`.

## 6. Cosa resta da fare

1. Architettura del plugin: caricare le traduzioni da un file esterno rilettura
   a ogni avvio (come su *Shadows*), e **ricordarsi la lingua scelta**, visto che
   il gioco non la salva.
2. Decidere il formato dei blocchi di traduzione e adattare `apply.py` di
   *Shadows*, con il controllo di lunghezza più stretto.
3. Aprire il repo, con l'identità git giusta.

Il conteggio del punto 2 di `START-HERE.md` è fatto e la via di iniezione è
provata: **si può cominciare a tradurre.**

## Strumenti prodotti

| File | Cosa fa |
|---|---|
| `tools/extract_i2.py` | estrae e parsa `I2Languages` da `data.unity3d` → CSV + JSON |
| `tools/decode_catalog.py` | decodifica il catalogo Addressables (chiavi, tipi, provider) |

Dipendenza: `UnityPy` (`pip install UnityPy`). Il dump di `GameAssembly.dll` è
stato fatto con Il2CppDumper 6.7.46, che non serve conservare: si riscarica.

Il plugin sonda sta in `plugin/`, si compila con `dotnet build -c Release` e ha
l'interruttore `InjectItalian` nella config di BepInEx: messo a `false` fa girare
il gioco intatto tenendo accesa la diagnostica. È servito due volte per
distinguere i nostri difetti da quelli del gioco, ed è la prima cosa da usare
davanti a un errore.

**Verificare sempre il checksum del DLL dopo averlo copiato in `BepInEx/plugins/`.**
Una build vecchia rimasta lì mi è costata tre esecuzioni di conclusioni sbagliate:
il gioco eseguiva un plugin diverso da quello appena scritto, e i log sembravano
smentire una correzione che invece non era mai stata caricata.

    md5sum plugin/bin/Release/net6.0/RonyItalian.dll \
           ../RoNY-game-copy/BepInEx/plugins/RonyItalian.dll
