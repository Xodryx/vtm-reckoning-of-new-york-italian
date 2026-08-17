# Stato del lavoro

Aggiornato il 17 agosto 2026. **10.394 battute tradotte su 11.141 (93,3%)**,
880.280 caratteri su 932.478 (94,4%). Il conteggio nel README lo aggiorna da sé
`apply.py`.

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

## Cosa manca, in ordine

**Il demo di Cracovia è tradotto, ma sappi cos'è.** Le 747 battute di
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

1. **Una vera prova di release.** `tools/release.sh` è scritto e provato — costruisce
   lo zip, controlla i blocchi prima di compilare, e con `--with-bepinex` include
   BepInEx rifiutandosi di farlo se manca il testo della licenza. Non è mai stato
   provato con l'archivio vero di BepInEx (i test usavano zip finti), e nessuno ha
   ancora installato il pacchetto su una macchina pulita seguendo il `LEGGIMI.txt`.
   Va fatto prima di pubblicare qualsiasi cosa.

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

## BUG APERTO: il plugin rompe la selezione del personaggio

In quella schermata la descrizione di Kali resta inglese e al posto di quella di
Pádraic compare la chiave `UI/MainMenu/Rony/PadraicDescription`. **È colpa nostra.**

Nel gioco **non modificato in francese quella schermata funziona**: la descrizione di
Kali è in francese e al posto della chiave compare, correttamente,
`PadraicUnlockRequirements` — *«Complétez l'histoire de Kali pour débloquer le deuxième
personnage.»* Quindi il pannello si localizza benissimo, finché non arriviamo noi.

**Attenzione a come si prova.** Cambiare lingua dal menù a gioco avviato non aggiorna
quella schermata: restano i testi della lingua precedente, e per un po' ho scambiato
quei residui per un difetto del francese. Si riconosce dalla filigrana in basso a
destra, che resta nella lingua di prima. **Ogni prova va fatta riavviando il gioco
nella lingua da testare.**

Cosa è già stato escluso, per non rifarlo:

- Il termine `KaliDescription` è tradotto e viene richiesto due volte; la patch su
  `TermData.GetTranslation` risponde con l'italiano entrambe le volte (verificato
  registrando valore in ingresso e in uscita).
- Le due sorgenti indicizzano l'italiano allo stesso modo (`italian=2` entrambe).
- Riempire la colonna italiana con `SetTranslation` prima che qualsiasi schermata
  esista **funziona** — 11.152 celle per sorgente, nessun crash, contrariamente a
  quanto teme il commento in `TranslationStore.cs`, che si riferisce a un tentativo di
  scrittura diretta sull'array. A schermo però non cambia niente, quindi è stato tolto:
  è codice rischioso senza un beneficio dimostrato.
- `PadraicDescription` non viene **mai** richiesta, in sei avvii.
- Una patch su `TMP_Text.set_text` non intercetta nessuna scrittura di quei testi.

Quindi il sospetto non è più su cosa rispondiamo alle letture — quello funziona — ma su
**cosa cambiamo nella sorgente**: `AddLanguage` allarga di una casella l'array di ogni
termine, `UpdateDictionary(true)` lo ricostruisce, e impostiamo
`OnMissingTranslation = Fallback`. Una di queste tre cose disturba il pannello.

Il prossimo passo è isolarla: **avviare il gioco moddato direttamente in francese.** Se
si rompe anche il francese, la causa è la registrazione della lingua e non la
traduzione, e va cercata fra quelle tre righe di `LanguageRegistration.Register`.

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
