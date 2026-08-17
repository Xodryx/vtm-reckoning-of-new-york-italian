# Stato del lavoro

Aggiornato il 17 agosto 2026. **11.141 battute tradotte su 11.141 (100%)**,
932.478 caratteri su 932.478. Il conteggio nel README lo aggiorna da sé `apply.py`.
Di *Reckoning of New York* sono 10.394: le altre 747 sono il demo di Cracovia,
che non è di questo gioco (vedi sotto).

**Non ci sono bug aperti.** La schermata di selezione del personaggio, che ci ha
tenuti impegnati a lungo, è risolta: non era colpa nostra ed era un difetto del gioco
non modificato. Vedi la sezione apposita — contiene anche due conclusioni sbagliate che
avevo scritto qui, tenute apposta per non ripercorrerle.

Questo documento serve a riprendere il lavoro senza rileggere tutto il resto.

## Dove siamo

**L'infrastruttura è finita e provata in gioco.** Il plugin funziona, il gioco parte
già in italiano, il flusso di traduzione ha la validazione automatica. Da qui in
avanti è solo traduzione: non c'è più reverse engineering da fare.

**La traduzione è completa: 11.141 su 11.141.** Comprese le 747 battute del demo di
Cracovia, che non è contenuto di questo gioco (vedi sotto) ma è stato tradotto per
completezza. Le uniche chiavi non tradotte sono le undici che nell'originale sono
vuote. Il conto del solo *Reckoning of New York* è **10.394**.

**Attenzione a come si conta ciò che manca.** `next_block.py` raggruppa sul
secondo pezzo della chiave e mostra i primi trenta gruppi: un gruppo da una o due
voci resta invisibile sotto quella soglia anche se il suo insieme è enorme. È così
che per settimane sono rimasti fuori radar il glossario in gioco (86 voci
`Glossary/<Termine>/Label` e `/Description`, cioè tutte le finestrelle che si
aprono cliccando i `<link>` del testo), gli 87 cartellini dei parlanti in
`ActorsDatabase` (quelli che stampavano *Sheriff*, *Harpy* e *Hound* sopra ogni
battuta) e i 49 obiettivi Steam. Per sapere davvero cosa manca conviene contare
per primo livello di chiave, non fidarsi dell'elenco:

    python -c "import json,collections; d=json.load(open('dump/i2_terms.json',encoding='utf-8'))['Terms']; it=json.load(open('translations/italian.json',encoding='utf-8')); print(collections.Counter(t['Term'].split('/')[0] for t in d if t['Term'] not in it))"

**Nel dump c'è anche il francese ufficiale.** `dump/i2_terms.json` porta due lingue,
inglese e francese, e il francese marca il genere dove l'inglese lo nasconde: è così
che si è stabilito che il narratore del primo intermezzo è un uomo e che quella del
terzo è una donna. **Non è però una fonte autorevole**: nell'undicesimo intermezzo
rende Torque al femminile, mentre l'inglese dice *«He can play it cool»* e *«his
yellow eyes»*. Va usato come indizio da verificare, mai come traduzione da copiare.

| Fatto | |
|---|---|
| Interfaccia | **completa** (627 stringhe) |
| Trama principale (`R_NIGHT_1`…`R_NIGHT_5`, `R_NIGHT_MQ_*`) | **completa** |
| Notte 1, missioni | **complete**: `REBUKE`, `UPINTHEAIR`, `LIAISONS` |
| Notte 2, missioni | **complete**: A for Anarchy, Empire of the Sun, The Night Courier, Spinfluence, `RUDEAWAKEN` |
| Notte 3, missioni | **complete**: Oh Brother, Connecting the Dots, `LITTLELEPER`, `CHASING_CHURCH`, `CHASING_PARK` |
| Notte 4, missioni | **complete**: entrambi i rami (`LLINE` e `FULLB`), le due code `WMAYBE` e i due `RIVERTWICE` |
| Notte 5, missioni | **complete**: `SYSCOLLAPSE` e `BADOMENS`, entrambe in doppia versione |
| Notte 6, missioni | **complete**: `DEATHAFTER`, `ETERSUNSHINE` in doppia versione, `CONVERS` |
| Notte 7, missioni | **complete**: `BURNINGMAN` (433, la più lunga del gioco), `SHADOWDAY`, `WEEKNIGHTMARES`, `BURNED` |
| Notte 8 ed epilogo | **completo**: `FROMASHES` |
| Finali alternativi | **completi**: `BEAST_ENDING`, `FALSE_ENDING`, `HUNTED_ENDING` |
| Intermezzi | **completi**: `R_INTERMISSION_1`…`_12` |
| Missioni facoltative | **complete**: `MQ_LUCKBEALADY`, `MQ_DREADGAME`, `MQ_BLOODANDCIGS`, `MQ_PROMISEMONSTER` |
| Agguati e incontri | **completi**: i tre `R_AMBUSH_*`, i quattro `FO_*` |
| Diario e registro | **completi**: `Journal/CONTACTS`, `Journal/LOGBOOK`, `Quest/MSQ`, `Quest/MSQ-2`, `Quest/ZONE` |
| Voci interne | **complete**: tutte le tracce `InnerVoices/*`, VOICES e THEONEVOICE |
| Glossario in gioco | **completo**: 86 voci, le finestrelle dei `<link>` |
| Cartellini dei parlanti | **completi**: `ActorsDatabase`, 87 voci |
| Obiettivi Steam | **completi**: `Achievements`, 24 titoli con descrizione |
| Cartelli a schermo | **completi**: `TextPanels`, il conto alla rovescia e gli stacchi |

## Come si riprende

    python tools/next_block.py                 # cosa resta, per conversazione
    python tools/next_block.py <gruppo> -n 65 --write
    # riempi blocks/block_NNN.json
    python tools/apply.py
    bash tools/deploy.sh

Il dettaglio sta in `FLUSSO.md`. **Nota**: `dump/` non è nel repo, va rigenerato con
`tools/extract_i2.py` da una copia del gioco, altrimenti gli strumenti non partono.

**Le missioni vanno tradotte a coppie.** Ogni missione esiste in due versioni,
`R_NIGHTn_MSQ_NOME` (Kali) e `M_NIGHTn_MSQ2_NOME` (Pádraic): sono la stessa scena
raccontata dai due protagonisti e condividono molte battute. Tradurle di seguito
evita di rendere due volte, in modo diverso, la stessa riga.

**Attenzione ai messaggi di commit da PowerShell.** Le virgolette doppie dentro un
here-string vengono rimangiate quando l'argomento passa a `git.exe`, e il commit
fallisce con un errore di pathspec. Scrivi il messaggio in un file e usa `git commit -F`.

## Decisioni di traduzione già prese

- **Kali è donna**, prima persona, presente, colloquiale e sardonica. Concordare al
  femminile: *"sono andata"*, *"mi ha messa in allerta"*. **Vale anche quando sono
  gli altri a parlarle**: D'Angelo le diceva *"sei stato tu a chiedermi"*, ed è
  sfuggito fino alla prova in gioco. L'inglese non marca il genere alla seconda
  persona, quindi l'errore non si vede nel testo di partenza: va cercato apposta.
  Un rastrellamento su `sei stato`, `ti sei`, `sarai`, `eri`, `fossi`, `ti ho` e
  simili nei blocchi costa poco e va rifatto ogni tanto.
- **Pádraic è formale e misurato**, prima persona, maschile. Dove Kali dice
  *"stronzate"*, lui dice *"accidenti"*.
- **Dove l'inglese alza il tiro, l'italiano lo segue.** `"pull an answer out of my
  ass"` è *"tirare fuori una risposta dal culo"*, non un eufemismo. Confermato
  dall'utente, ed è la stessa regola adottata per Julia su *Shadows*.
- **D'Angelo** ha la parlata da detective anni Quaranta; Kali lo sfotte chiamandolo
  *"il piccolo Chandler"*.
- La terminologia si consulta: `reference/official_glossary.json` prima, poi
  `reference/supplementary_glossary.json`, che dichiara la fonte voce per voce.
- **I giochi di parole sul nome Hope si esplicitano.** In inglese *hope* è anche
  una parola comune, in italiano no: dove il testo ci gioca sopra, l'italiano dice
  entrambe le cose — *«la nostra ultima speranza. Anzi, la nostra ultima Hope»*.
- **Le lingue straniere restano straniere**: l'irlandese di Pádraic (*As ucht Dé*,
  *Naomh Pádraig*) e lo spagnolo con cui Kali sbaglia la lingua di Kaiser
  (*No hablo español* → *Vorrai dire alemán*) sono la battuta, non l'ostacolo.
- **Il vero nome di Kali è California**, rivelato da Hope nella quarta notte.
- **Gli indovinelli che nascondono un toponimo inglese restano in inglese nelle
  parole-chiave.** Nella terza notte una preghiera cela *North Brother Island* e
  tre righe successive scandiscono «North.» «Brother.» «Island.». Tradurle
  spezzerebbe l'indovinello, perché l'isola si chiama così anche in italiano: le
  tre parole restano in inglese dentro la preghiera, il resto è tradotto.
- **`reckoning` è sempre *resa dei conti***, la parola del titolo. Torna nel nome
  della settima notte, nelle ultime righe di Kali (*«questa è la mia fottuta resa
  dei conti»*) e in quelle di Pádraic, che le fanno eco (*«mi hai concesso la mia
  resa dei conti»*). Le due chiuse vanno lette insieme: sono la stessa frase.
- **Il genere va controllato anche sui personaggi di contorno.** Julia Sowinski è
  la Primogenita Lasombra, e l'inglese usa `them` proprio per non marcarla; in tre
  battute della terza notte era diventata «il Primogenito». Stessa trappola di
  Kali, stessa cura.
- **`caretaker` è sempre *custode***, il ruolo che il Consiglio assegna a Pádraic
  nella terza notte. Era sfuggito un *protettore* in `FALSE_ENDING`, corretto.
- **Le citazioni nei titoli dei capitoli si rifanno al titolo italiano dell'opera**,
  tranne quando quel titolo distruggerebbe il senso: `Eternal Sunshine of Naivety`
  è *Eterno splendore dell'ingenuità* (dal verso di Pope), non *Se mi lasci ti
  cancello*. `Burning Man` e `Burned Man` restano in inglese perché sono una coppia.
- **Il *thunderword* di Joyce non si tocca.** Trenta righe di `VOICES_SHADOWDAY`
  sono `kamminarronnkonnbronntonnerronntuonn`, il centro del primo tuono di
  *Finnegans Wake*. Pádraic è «un patito di James Joyce» per esplicita ammissione
  del diario: la parola è intraducibile in ogni lingua, inglese compreso.
- **I titoli di canzone restano in inglese**, come `Burning Man` e `Empire of the
  Sun`: `Stranger in the Night` e `Luck Be A Lady Tonight` sono Sinatra, e in Italia
  si conoscono con quel nome. Il francese ufficiale li traduce; noi no.
- **I cartellini dei parlanti seguono le schede dei contatti.** `ActorsDatabase`
  e `Journal/CONTACTS` nominano le stesse persone: dove la scheda dice *Giocatore
  di strada* o *Profeta di sventura*, il cartellino dice lo stesso.
- **I nominativi radio dei cacciatori si traducono**: `Lightbringer` è
  *Portaluce*. Sono nomi parlanti, non cognomi: nella quinta notte la voce alla
  radio dice che «la luce scaccerà le tenebre», e in inglese il gioco si sente.
  L'acronimo `BFB` resta invece com'è, come la sigla di un'arma.

## Il demo di Cracovia

**È tradotto, ma sappi cos'è.** Le 747 battute di
`CardinalTMP/*` (714), `VariaCRD/*` (25) e `ActorsCRD/*` (8) sono un altro progetto
Draw Distance rimasto nella tabella: i personaggi sono Rosalind Davis, Rosa, Radek,
Mirek; l'ambientazione è **piazza Podgórze a Cracovia**; e `VariaCRD/CRDZone/DemoEnd`
dice testualmente *«This concludes the demo.»*. `TMP` sta per *temporary*. Nessun
giocatore di RoNY lo vedrà mai: è tradotto perché il contatore fosse pieno e onesto.

Tre scelte prese lì dentro, che valgono se quel materiale tornasse mai utile: il
**polacco resta polacco** (Rosa ne capisce sì e no una parola, e lo dice); la lingua
di Rosa resta **«inglese»** anche se il testo è italiano, perché il vicino è un
professore d'inglese in pensione e cambiarlo sfascerebbe la scena; e lo `statist` del
palo — dal polacco *statysta*, «comparsa» — diventa **statista**, che in italiano è
lo stesso identico falso amico.

Il controllo CI c'è (`.github/workflows/check.yml`) e la pipeline di release è
`tools/release.sh`; il perché di certe scelte sta in `ARCHITETTURA.md`. Due vincoli
che è meglio non riscoprire da capo:

- **`apply.py` non aveva modo di girare senza `dump/`**, che resta fuori dal repo
  perché è testo protetto. Ora si appoggia a `reference/english_fingerprints.json`,
  che porta solo l'*impronta* di ogni riga inglese — tag, segnaposto, conteggi,
  lunghezza — e viene rigenerata dal dump a ogni scrittura, così non va fuori
  sincrono. È questo che permette alla CI di fare lo stesso controllo.
- **La DLL non è compilabile su GitHub.** Il plugin referenzia i ~152 assembly
  interop che BepInEx genera dai metadati IL2CPP del gioco: derivano da una copia del
  gioco, quindi nessun runner ospitato può produrli. La release si costruisce in
  locale, punto.

**Lasciate volutamente in inglese** (o in polacco), come le 19 descrizioni di
missione che il gioco spedisce col nome della propria chiave: le 26 battute di
prova degli sviluppatori (`Dialogue_0`…`Dialogue_16`, `Dialogue_ImVampire`,
`Przykra sprawa kurde faja`), i segnaposto `ActorsDatabase/Actor_1` e il
`Lorem Ipsum` dell'interfaccia. Stanno nei blocchi con il valore originale, così
il conteggio è onesto e a runtime non cambia nulla.

## Risolto: le descrizioni della selezione del personaggio

Si vedeva la descrizione di Kali in inglese e, al posto di quella di Pádraic, la chiave
`UI/MainMenu/Rony/PadraicDescription` stampata così com'è.

**Non era colpa nostra: è un difetto del gioco non modificato.** Le due etichette
`Description` di quel pannello non hanno nessun componente `I2.Loc.Localize` addosso e
nessun codice ci scrive dentro. Mostrano quello che era stato scritto a mano nel prefab
in editor: l'inglese sotto Kali, la chiave sotto Pádraic. Un giocatore inglese vede la
chiave grezza esattamente come la vedevamo noi.

Conferma di contorno, nello stesso pannello: le etichette dei suggerimenti di
navigazione nei prefab «Template» contengono `Parcourir`, `Confirmer`, `Retour` —
francese cotto dentro, in una sessione che non è mai stata in francese. I prefab di
questo gioco si portano dietro il testo dell'ultima anteprima fatta in editor, in lingue
assortite.

### Cosa si è imparato per strada

- **I2 ha tre vie di lettura, non due.** Coprivamo `I2LocalizationDatabase.GetValue` e
  `TermData.GetTranslation`; mancava del tutto quella che usano i componenti `Localize`
  piazzati in scena, cioè `LanguageSourceData.TryGetTranslation` e sopra di essa
  `LocalizationManager.GetTranslation` / `GetTermTranslation`. Ora sono coperte tutte.
- **Una postfix che scrive in un parametro per riferimento non arriva al chiamante
  nativo.** La prima patch consegnava l'italiano nel parametro `Translation` di
  `TryGetTranslation`, il log confermava la consegna, e a schermo non cambiava niente.
  Le patch che funzionano scrivono tutte nel valore di ritorno (`__result`). Da
  ricordare: **il log può annunciare una traduzione che non è andata da nessuna parte.**
- **Le firme esatte si estraggono dai metadati interop**, senza decompilatori:
  `grep -a -o -E "NativeMethodInfoPtr_[A-Za-z0-9_]+" BepInEx/interop/NOME.dll | sort -u`
  restituisce nome, visibilità, tipo di ritorno e parametri di ogni metodo.
- La prova decisiva è stata far elencare al plugin le etichette del pannello con il loro
  contenuto: `LogLocalizationDetail = true` nel config. Le vicine tenevano italiano,
  quelle due no.

### La correzione

`plugin/CharacterPanelText.cs` riempie le due etichette quando il titolo del pannello
viene localizzato. Il testo non è indovinato: il gioco **chiede** i termini giusti
mentre apre il pannello — `KaliDescription` per lei, `PadraicUnlockRequirements` finché
lui è bloccato e `PadraicDescription` dopo — e poi non li scrive da nessuna parte. Noi
seguiamo quello che chiede, quindi continuerà a funzionare quando lo sbloccherai.

**È l'unico punto in cui il plugin scrive nella scena invece di rispondere a una
lettura.** Non era evitabile: non c'è nessuna lettura da intercettare, perché nessuno
legge quelle etichette. Ha un interruttore suo, `FixCharacterPanel`, e nessun file del
gioco viene toccato — la regola grossa resta intatta.

### Il setaccio, e come si legge

`ReportUntranslatedLabels = true` nel config accende un rilevatore che segnala il testo
a schermo che la traduzione non può raggiungere. Serve, ma **va letto con prudenza**:
quasi tutto quello che trova è innocuo.

Il gioco assegna ai componenti `I2.Loc.Localize` **il testo già risolto** invece della
chiave, e i suoi prefab contengono testo di riempimento scritto in editor. Il risultato
è che il setaccio vede fotogrammi intermedi che nessun giocatore vedrà mai: sulla stessa
etichetta `QuestSubtitle` compaiono prima `Main Quest` e poi «Missione principale».
**Regola: ogni stringa inglese trovata va cercata nella tabella dei termini prima di
crederle.** Se la chiave esiste ed è tradotta, è un segnaposto e si ignora.

Nella prima campagna su una sessione di gioco vera, di 56 segnalazioni **una sola** era
reale: `New Glossary entry unlocked!`, il segnaposto della notifica condivisa
`P_Notification_Codex`, che non esiste nella tabella dei termini e che il gioco a volte
mostra senza scriverci sopra. Sta in `BakedText.cs`. Le altre 55 erano `Option Name`,
`Person`, `Prop1`, gli slot di salvataggio: tutte spente, tutte riscritte dal codice.

Un difetto vero ma non nostro, trovato per strada: `VersionNumber = 'v. ??? -'`. Il
gioco non riempie mai il proprio numero di versione.

### Due conclusioni sbagliate che avevo scritto qui

Restano scritte perché non vengano ripercorse.

1. *«Il testo è cotto nella scena.»* Giusta nella sostanza, l'avevo scartata per la
   ragione sbagliata: il francese funziona **altrove**, non su quelle etichette.
2. *«È la registrazione della lingua a rompere il pannello, e il colpevole è
   `AddLanguage`.»* Falsa. `AddLanguage`, `UpdateDictionary(true)` e
   `OnMissingTranslation = Fallback` non c'entrano niente, e il test a interruttori che
   avevo progettato qui non serve a nulla. Nasceva da uno screenshot in francese
   interpretato male.

Vale ancora, invece, l'avvertenza sul metodo: **cambiare lingua a gioco avviato non
aggiorna quella schermata**, restano i testi della lingua precedente e sembrano un
difetto della lingua nuova. Si riconosce dalla filigrana in basso a destra, che resta
indietro. Ogni prova va fatta riavviando.

E questo è vero ma inutile: riempire la colonna italiana con `SetTranslation` funziona
davvero — 11.152 celle per sorgente, nessun crash, al contrario di quel che teme il
commento in `TranslationStore.cs`, che si riferisce a una scrittura diretta sull'array.
Non serve a niente, ed è stato tolto.

## Cosa manca, in ordine

1. **Una vera prova di release.** `tools/release.sh` costruisce lo zip, controlla i
   blocchi prima di compilare e con `--with-bepinex` include BepInEx rifiutandosi di
   farlo se manca il testo della licenza. Provato solo con archivi BepInEx finti, mai
   con quello ufficiale, e mai installato su una macchina pulita seguendo il
   `LEGGIMI.txt`. Da fare prima di pubblicare qualsiasi cosa.

## Le voci lasciate identiche all'inglese

Sono 361, di cui 242 fuori dai dialoghi, e **quasi tutte sono giuste**: nomi propri
(Kali, Pádraic, Ravnos, New York), clan e Discipline che l'italiano ufficiale non
traduce (Malkavian, Auspex), tasti dei controller (LB, RT, LS), parole che in italiano
si dicono così (VSync, Codex, Auto, Max, Ultra, Tutorial), i segnaposto *Lorem Ipsum*
del gioco, e le citazioni dal *Finnegans Wake* nelle voci interne
(`kamminarronnkonnbronnto…`, la parola-tuono) che vanno lasciate intatte.

Ci si nascondeva però una svista vera, trovata giocando: `UI/Misc/Select` era rimasto
`Select`, ed è l'etichetta che compare accanto alla barra spaziatrice. Ora è
*Seleziona*. **Il francese è il modo rapido per separare le due categorie**: dove il
francese traduce e noi no, va guardato.

Un caso resta in sospeso di proposito: `UI/Misc/Start` è ancora `Start`. Il francese
dice *Commencer*, ma quel termine sta in mezzo ai nomi dei tasti dei controller, dove
«Start» è il nome del pulsante e tradurlo sarebbe sbagliato. Non l'abbiamo mai visto a
schermo: se salta fuori come azione, va reso *Avvia*.

## La prova di release, e cosa ha trovato

Fatta il 17 agosto 2026 su una copia vergine del gioco presa da Steam, installando solo
il pacchetto e seguendo il `LEGGIMI.txt` alla lettera. Ha trovato quattro difetti nostri,
tutti corretti — che è esattamente perché andava fatta invece di darla per buona.

1. **Gli archivi ufficiali di BepInEx non contengono nessuna licenza.** Verificato su
   `6.0.0-be.785`: 233 file, nessun `LICENSE`, nessun `COPYING`. La LGPL-2.1 non obbliga
   loro, obbliga chi ridistribuisce i binari: noi. Il testo canonico ora sta in
   `reference/bepinex-license.txt` (da gnu.org, integro) e viaggia nel pacchetto come
   `BepInEx-LICENSE.txt`.
2. **Il `LEGGIMI` prometteva il falso**: «il gioco parte già in italiano». Su
   un'installazione pulita non c'è nessun config, quindi il plugin lasciava scegliere al
   gioco. Sembrava vero solo perché la nostra copia di lavoro ricordava `it` dalle prove.
   Ora `LastLanguage` nasce a `it`.
3. **Un solo `LEGGIMI` per due pacchetti diversi**: diceva a chi aveva BepInEx incluso di
   andarselo a installare, e a chi non ce l'aveva niente su come rimuoverlo.
4. **Le due varianti avevano lo stesso nome di file** e si sovrascrivevano in silenzio.
   Ora sono `RonyItalian-ita-v0.1.0.zip` (459 KB) e `RonyItalian-ita-v0.1.0-con-bepinex.zip` (34 MB).

**Come si rifà.** Copia il gioco da Steam in una cartella nuova, estraici il pacchetto,
e **aggiungi `steam_appid.txt` con dentro `2658720`**: senza, la SDK di Steam fa
riavviare il gioco tramite Steam, che lancia la *sua* copia e la prova non prova niente.
Non va nel pacchetto: un giocatore vero installa dentro la cartella di Steam, dove il
problema non esiste.

## Numerazione delle versioni

**Le versioni pubblicate restano sotto la 1.0 finché la traduzione non è stata riletta
da una persona giocandoci.** Decisione dell'utente, e ha ragione: il numero di versione
è l'unica cosa che un giocatore vede prima di scaricare, e dare 1.0 a un testo mai
verificato prometterebbe una solidità che non c'è. **Completo e verificato non sono la
stessa cosa**, e qui siamo al primo.

In pratica: `Version` in `plugin/Plugin.cs` resta `0.x` — da lì il numero si propaga da
solo al nome dello zip e alla riga di log. La prima pubblica può essere `0.9.0`, poi
`0.9.1`, `0.9.2` a ogni correzione. La `1.0.0` solo dopo una partita giocata per intero.
Il `LEGGIMI.txt` lo dice esplicitamente al giocatore e spiega come segnalare gli errori.

## Le licenze del pacchetto con BepInEx

Verificate una per una il 17 agosto 2026, interrogando ogni progetto a monte. **Non
andare a memoria su questo**: due risultati non erano quelli che mi aspettavo.

- **Gli archivi ufficiali di BepInEx non contengono nessun file di licenza.** Li mette
  il pacchetto: `reference/licenses/`, un file per progetto, preso dal `LICENSE`
  originale con le sue note di copyright.
- **«BepInEx» sono sedici progetti**, non uno: BepInEx (LGPL-2.1), Il2CppInterop
  (LGPL-3.0), UnityDoorstop (LGPL-2.1), Dobby (Apache-2.0), il runtime .NET e altri
  undici sotto MIT.
- **Il2CppInterop risulta `NOASSERTION` all'API di GitHub**, ma il file è la LGPL-3.0:
  contiene solo l'addendum, senza il testo GPL-3 a cui si aggancia, e il classificatore
  non lo riconosce. Va letto, non chiesto.
- **AssetRipper è GPL-3.0 e sarebbe stato un problema**, ma i due file impacchettati —
  `AssetRipper.CIL.dll` e `AssetRipper.Primitives.dll` — stanno in repository separati e
  sono entrambi MIT. È il controllo che è servito di più.

`LICENZE.txt` nel pacchetto mappa ogni file al suo progetto e alla sua licenza, con i
link ai sorgenti. Se un domani si aggiorna la build di BepInEx, **quella tabella va
rifatta**: i componenti cambiano.

## Le due cose che servono da un umano

Nessun controllo automatico può darle, e sono l'unico punto in cui il lavoro è
davvero bloccato senza di te.

1. **Una partita giocata in italiano.** Ormai è tradotto tutto ciò che si incontra
   giocando: solo giocandolo si capisce se il registro di Kali regge sulla lunga
   distanza. Il validatore sa dire che un `<link="Sire">` è integro, non che una
   battuta sarcastica suona sincera.
2. **Un riscontro sui termini marcati `unverified`** in
   `reference/supplementary_glossary.json` — `Daystar` → *Astro Diurno*,
   `Lingering Kiss` → *Bacio Persistente*, `Compel` → *Comando*, `praxis`
   invariato, e i due soprannomi Ravnos che il Compendio non conferma,
   `Daredevils` → *Spericolati* e `The Haunted` → *Braccati*. Se hai i manuali
   italiani, sono quelli da controllare: compaiono ovunque e un errore si propaga
   per tutte le 11.000 righe.

   **Si è aggiunto un gruppo intero: `clan_monikers`.** Ogni scheda di clan del
   glossario in gioco si chiude con una riga di appellativi — una novantina di
   termini, dal *Clan della Caccia* dei Banu Haqim ai *Voivodi* Tzimisce. Il manuale
   italiano li ha tutti, ma non è stato raggiungibile: `worldofdarkness.it` non
   risolve e la scansione su AnyFlip risponde 403. *Clan dei Re* e *Sangue Blu* sono
   confermati da fonti secondarie; **tutto il resto è una nostra resa** e sta lì
   marcato `unverified`. È il singolo blocco di terminologia più grosso che manca
   di riscontro.

   Le ricerche in rete hanno già confermato *Corvi* e *Canaglie* per i Ravnos
   (Compendio italiano), *Mesmerismo*, *Rivelare il Temperamento*, *Passaggio
   Inosservato*, *Percepire l'Invisibile*, *Melpominee* invariato, *Vicissitudine*
   e le sei Tradizioni (*La Masquerade, Il Dominio, La Progenie, La Responsabilità,
   L'Ospitalità, La Distruzione*).

Il precedente vale come monito: tre nomi di Discipline su cinque erano sbagliati
finché non li abbiamo verificati su una fonte. *Animalismo*, *Offuscamento* e
*Presenza* sembravano corretti e non lo erano.
