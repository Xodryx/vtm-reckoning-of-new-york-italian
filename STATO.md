# Stato del lavoro

Aggiornato il 16 agosto 2026. **784 battute tradotte su 11.141 (7,0%)**, 38.637
caratteri su 932.478 (4,1%). Il conteggio nel README lo aggiorna da sé `apply.py`.

Questo documento serve a riprendere il lavoro senza rileggere tutto il resto.

## Dove siamo

**L'infrastruttura è finita e provata in gioco.** Il plugin funziona, il gioco parte
già in italiano, il flusso di traduzione ha la validazione automatica. Da qui in
avanti è solo traduzione: non c'è più reverse engineering da fare.

| Fatto | |
|---|---|
| Interfaccia | **completa** (627 stringhe) |
| `Dialogue/R_NIGHT_1` | **completa** (144 battute), prima conversazione del gioco |
| `Dialogue/R_NIGHT_3` | in corso, 85 su 242 |

## Come si riprende

    python tools/next_block.py                 # cosa resta, per conversazione
    python tools/next_block.py R_NIGHT_3 -n 50 --write
    # riempi blocks/block_NNN.json
    python tools/apply.py
    bash tools/deploy.sh

Il dettaglio sta in `FLUSSO.md`. **Nota**: `dump/` non è nel repo, va rigenerato con
`tools/extract_i2.py` da una copia del gioco, altrimenti gli strumenti non partono.

## Decisioni di traduzione già prese

- **Kali è donna**, prima persona, presente, colloquiale e sardonica. Concordare al
  femminile: *"sono andata"*, *"mi ha messa in allerta"*.
- **Dove l'inglese alza il tiro, l'italiano lo segue.** `"pull an answer out of my
  ass"` è *"tirare fuori una risposta dal culo"*, non un eufemismo. Confermato
  dall'utente, ed è la stessa regola adottata per Julia su *Shadows*.
- **D'Angelo** ha la parlata da detective anni Quaranta; Kali lo sfotte chiamandolo
  *"il piccolo Chandler"*.
- La terminologia si consulta: `reference/official_glossary.json` prima, poi
  `reference/supplementary_glossary.json`, che dichiara la fonte voce per voce.

## Cosa manca, in ordine

1. **La traduzione**: 10.357 battute, il 96% dei caratteri.
2. Un controllo CI che esegua `tools/apply.py --check` a ogni push.
3. La pipeline di release. Vincoli in `ARCHITETTURA.md`.

## Le due cose che servono da un umano

Nessun controllo automatico può darle, e sono l'unico punto in cui il lavoro è
davvero bloccato senza di te.

1. **Una partita giocata in italiano.** `R_NIGHT_1` è completa: solo giocandola si
   capisce se il registro di Kali regge sulla lunga distanza. Il validatore sa dire
   che un `<link="Sire">` è integro, non che una battuta sarcastica suona sincera.
2. **Un riscontro sui termini marcati `unverified`** in
   `reference/supplementary_glossary.json` — `Raven` → *Corvo*, `Eternal Struggle` →
   *Eterna Lotta*, `praxis` invariato. Se hai i manuali italiani, sono quelli da
   controllare: compaiono ovunque e un errore si propaga per tutte le 11.000 righe.

Il precedente vale come monito: tre nomi di Discipline su cinque erano sbagliati
finché non li abbiamo verificati su una fonte. *Animalismo*, *Offuscamento* e
*Presenza* sembravano corretti e non lo erano.
