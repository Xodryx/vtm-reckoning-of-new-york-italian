# Stato del lavoro

Aggiornato il 17 agosto 2026. **9.669 battute tradotte su 11.141 (86,8%)**,
821.677 caratteri su 932.478 (88,1%). Il conteggio nel README lo aggiorna da sé
`apply.py`.

Questo documento serve a riprendere il lavoro senza rileggere tutto il resto.

## Dove siamo

**L'infrastruttura è finita e provata in gioco.** Il plugin funziona, il gioco parte
già in italiano, il flusso di traduzione ha la validazione automatica. Da qui in
avanti è solo traduzione: non c'è più reverse engineering da fare.

**Tutto il contenuto narrativo è tradotto**: le otto notti, l'epilogo, i due
finali alternativi, le missioni facoltative, gli agguati, il diario, il registro
delle missioni e le voci interne. Quello che resta sono code piccole — gli
intermezzi e una lunga fila di gruppi da poche battute — più `CardinalTMP`, che
non è contenuto di questo gioco (vedi sotto).

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
| Finali alternativi | **completi**: `BEAST_ENDING`, `FALSE_ENDING` |
| Missioni facoltative | **complete**: `MQ_LUCKBEALADY`, `MQ_DREADGAME`, `MQ_BLOODANDCIGS`, `MQ_PROMISEMONSTER` |
| Agguati e incontri | **completi**: i tre `R_AMBUSH_*`, i quattro `FO_*` |
| Diario e registro | **completi**: `Journal/CONTACTS`, `Journal/LOGBOOK`, `Quest/MSQ`, `Quest/MSQ-2`, `Quest/ZONE` |
| Voci interne | **complete**: le sei tracce `InnerVoices/*` |

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
- **I nominativi radio dei cacciatori si traducono**: `Lightbringer` è
  *Portaluce*. Sono nomi parlanti, non cognomi: nella quinta notte la voce alla
  radio dice che «la luce scaccerà le tenebre», e in inglese il gioco si sente.
  L'acronimo `BFB` resta invece com'è, come la sigla di un'arma.

## Cosa manca, in ordine

1. **La traduzione**: 1.472 battute, ma **solo ~790 sono di questo gioco**. Sono
   tutte code piccole e senza ordine narrativo: gli intermezzi (`R_INTERMISSION_1`
   … `_12`, 11-23 battute l'uno), qualche coda di missione (`R_NIGHT3_MSQ_WHEREVER`,
   `R_NIGHT2_MSQ_LASTRESORT`, `M_NIGHT7_MSQ2_OLDBOY`), `Quest/MQ` e `Quest/INT`, poi
   una fila lunga di gruppi da meno di dieci battute. Conviene raccoglierne
   parecchi in un blocco solo: `next_block.py <gruppo> -n N` senza `--write` stampa
   l'inglese senza creare lo scheletro, così si compone un file unico a mano.
2. **`CardinalTMP/*` (713 battute) non è contenuto di *Reckoning of New York*** e
   l'ho lasciato da parte in attesa di una decisione dell'utente. È materiale di un
   altro progetto Draw Distance rimasto nella tabella: i personaggi sono Rosa e
   Radek, l'ambientazione è **piazza Podgórze a Cracovia**, e certe righe non sono
   nemmeno in inglese (`CRD_NEIGHBOR/LINE-3` è polacco: *«Halo! Co to za łażenie
   ludziom po balkonach?»*). `TMP` sta per *temporary*. Non lo vedrà mai nessun
   giocatore: tradurlo è un quinto del lavoro rimasto speso a vuoto.
3. Un controllo CI che esegua `tools/apply.py --check` a ogni push.
4. La pipeline di release. Vincoli in `ARCHITETTURA.md`.

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

   Le ricerche in rete hanno già confermato *Corvi* e *Canaglie* per i Ravnos
   (Compendio italiano), *Mesmerismo*, *Rivelare il Temperamento*, *Passaggio
   Inosservato*, *Percepire l'Invisibile* e *Melpominee* invariato. Restano dubbi
   solo quelli qui sopra.

Il precedente vale come monito: tre nomi di Discipline su cinque erano sbagliati
finché non li abbiamo verificati su una fonte. *Animalismo*, *Offuscamento* e
*Presenza* sembravano corretti e non lo erano.
