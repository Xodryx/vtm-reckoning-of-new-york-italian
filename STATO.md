# Stato del lavoro

Aggiornato il 16 agosto 2026. **4.470 battute tradotte su 11.141 (40,1%)**,
365.862 caratteri su 932.478 (39,2%). Il conteggio nel README lo aggiorna da sé
`apply.py`.

Questo documento serve a riprendere il lavoro senza rileggere tutto il resto.

## Dove siamo

**L'infrastruttura è finita e provata in gioco.** Il plugin funziona, il gioco parte
già in italiano, il flusso di traduzione ha la validazione automatica. Da qui in
avanti è solo traduzione: non c'è più reverse engineering da fare.

**Tutta la linea narrativa principale è tradotta.** Non resta più nessuna
conversazione `Dialogue/R_NIGHT_*`: quello che manca sono le missioni secondarie
di ogni notte e il testo delle carte.

| Fatto | |
|---|---|
| Interfaccia | **completa** (627 stringhe) |
| Trama principale (`R_NIGHT_1`…`R_NIGHT_5`, `R_NIGHT_MQ_*`) | **completa** |
| Notte 2, missioni | **complete**: A for Anarchy (entrambe le versioni), Empire of the Sun, The Night Courier, Spinfluence |
| Notte 3, missioni | **complete**: Oh Brother e Connecting the Dots, entrambe in doppia versione |
| Notte 4, missioni | **complete**: entrambi i rami (`LLINE` e `FULLB`), le due code `WMAYBE` e i due `RIVERTWICE` |

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
- **I nominativi radio dei cacciatori si traducono**: `Lightbringer` è
  *Portaluce*. Sono nomi parlanti, non cognomi: nella quinta notte la voce alla
  radio dice che «la luce scaccerà le tenebre», e in inglese il gioco si sente.
  L'acronimo `BFB` resta invece com'è, come la sigla di un'arma.

## Cosa manca, in ordine

1. **La traduzione**: 6.671 battute, il 61% dei caratteri. Le prime quattro notti
   sono finite; si riprende dalla quinta con `M_NIGHT5_MSQ_SYSCOLLAPSE` (225) e il
   suo gemello `M_NIGHT5_MSQ2_SYSCOLLAPSE` (113). Le conversazioni più grosse
   rimaste sono `R_NIGHT7_MSQ_BURNINGMAN` (433) e `CardinalTMP/CRD_PONY1` (268);
   il testo delle carte (`CardinalTMP/*`) non è ancora stato toccato.
2. Un controllo CI che esegua `tools/apply.py --check` a ogni push.
3. La pipeline di release. Vincoli in `ARCHITETTURA.md`.

## Le due cose che servono da un umano

Nessun controllo automatico può darle, e sono l'unico punto in cui il lavoro è
davvero bloccato senza di te.

1. **Una partita giocata in italiano.** Tutta la trama principale è tradotta: solo
   giocandola si capisce se il registro di Kali regge sulla lunga distanza. Il
   validatore sa dire che un `<link="Sire">` è integro, non che una battuta
   sarcastica suona sincera.
2. **Un riscontro sui termini marcati `unverified`** in
   `reference/supplementary_glossary.json` — `Daystar` → *Astro Diurno*,
   `Lingering Kiss` → *Bacio Persistente*, `Compel` → *Comando*, `praxis`
   invariato. Se hai i manuali italiani, sono quelli da controllare: compaiono
   ovunque e un errore si propaga per tutte le 11.000 righe.

   Le ricerche in rete hanno già confermato *Corvi* e *Canaglie* per i Ravnos
   (Compendio italiano), *Mesmerismo*, *Rivelare il Temperamento* e *Passaggio
   Inosservato*. Restano dubbi solo i quattro qui sopra.

Il precedente vale come monito: tre nomi di Discipline su cinque erano sbagliati
finché non li abbiamo verificati su una fonte. *Animalismo*, *Offuscamento* e
*Presenza* sembravano corretti e non lo erano.
