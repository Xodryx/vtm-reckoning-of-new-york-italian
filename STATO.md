# Stato del lavoro

Aggiornato il 17 agosto 2026. **10.394 battute tradotte su 11.141 (93,3%)**,
880.280 caratteri su 932.478 (94,4%). Il conteggio nel README lo aggiorna da sé
`apply.py`.

Questo documento serve a riprendere il lavoro senza rileggere tutto il resto.

## Dove siamo

**L'infrastruttura è finita e provata in gioco.** Il plugin funziona, il gioco parte
già in italiano, il flusso di traduzione ha la validazione automatica. Da qui in
avanti è solo traduzione: non c'è più reverse engineering da fare.

**Di *Reckoning of New York* non resta più niente da tradurre.** Le 747 battute
che il contatore segna come mancanti sono tutte del demo di Cracovia (vedi sotto),
più undici chiavi che nell'originale sono vuote.

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

1. **Il demo di Cracovia: 747 battute, e non è *Reckoning of New York*.** Sono
   `CardinalTMP/*` (714), `VariaCRD/*` (25) e `ActorsCRD/*` (8), tutte dello stesso
   materiale: un altro progetto Draw Distance rimasto nella tabella. I personaggi
   sono Rosalind Davis, Rosa, Radek, Mirek; l'ambientazione è **piazza Podgórze a
   Cracovia**; certe righe non sono nemmeno in inglese (`CRD_NEIGHBOR/LINE-3` è
   polacco: *«Halo! Co to za łażenie ludziom po balkonach?»*); e `VariaCRD/CRDZone/DemoEnd`
   dice testualmente *«This concludes the demo.»*. `TMP` sta per *temporary*.
   Nessun giocatore di RoNY lo vedrà mai. **In attesa di una decisione dell'utente.**
2. Un controllo CI che esegua `tools/apply.py --check` a ogni push.
3. La pipeline di release. Vincoli in `ARCHITETTURA.md`.

**Lasciate volutamente in inglese** (o in polacco), come le 19 descrizioni di
missione che il gioco spedisce col nome della propria chiave: le 26 battute di
prova degli sviluppatori (`Dialogue_0`…`Dialogue_16`, `Dialogue_ImVampire`,
`Przykra sprawa kurde faja`), i segnaposto `ActorsDatabase/Actor_1` e il
`Lorem Ipsum` dell'interfaccia. Stanno nei blocchi con il valore originale, così
il conteggio è onesto e a runtime non cambia nulla.

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
