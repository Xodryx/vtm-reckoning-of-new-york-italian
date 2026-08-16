# Stato del lavoro

Aggiornato il 16 agosto 2026. **2.233 battute tradotte su 11.141 (20,0%)**,
167.050 caratteri su 932.478 (17,9%). Il conteggio nel README lo aggiorna da sé
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
  femminile: *"sono andata"*, *"mi ha messa in allerta"*.
- **Pádraic è formale e misurato**, prima persona, maschile. Dove Kali dice
  *"stronzate"*, lui dice *"accidenti"*.
- **Dove l'inglese alza il tiro, l'italiano lo segue.** `"pull an answer out of my
  ass"` è *"tirare fuori una risposta dal culo"*, non un eufemismo. Confermato
  dall'utente, ed è la stessa regola adottata per Julia su *Shadows*.
- **D'Angelo** ha la parlata da detective anni Quaranta; Kali lo sfotte chiamandolo
  *"il piccolo Chandler"*.
- La terminologia si consulta: `reference/official_glossary.json` prima, poi
  `reference/supplementary_glossary.json`, che dichiara la fonte voce per voce.

## Cosa manca, in ordine

1. **La traduzione**: 8.908 battute, l'82% dei caratteri. Le conversazioni più
   grosse sono `R_NIGHT7_MSQ_BURNINGMAN` (433), `R_NIGHT3_MSQ_OHBROTHER` (326) e
   `CardinalTMP/CRD_PONY1` (268).
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
   `reference/supplementary_glossary.json` — `Raven` → *Corvo*, `Rogue` →
   *Canaglia*, `Daystar` → *Astro Diurno*, `Unseen Passage` → *Passaggio
   Inosservato*, `praxis` invariato. Se hai i manuali italiani, sono quelli da
   controllare: compaiono ovunque e un errore si propaga per tutte le 11.000 righe.

Il precedente vale come monito: tre nomi di Discipline su cinque erano sbagliati
finché non li abbiamo verificati su una fonte. *Animalismo*, *Offuscamento* e
*Presenza* sembravano corretti e non lo erano.
