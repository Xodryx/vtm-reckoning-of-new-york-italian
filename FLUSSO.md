# Flusso di traduzione

Quattro comandi. Il testo inglese del gioco non entra mai nel versionamento: è
materiale protetto, e sta in `dump/`, che `.gitignore` esclude.

## 1. Scegliere cosa tradurre

    python tools/next_block.py

Elenca quanto resta, raggruppato per conversazione. Poi:

    python tools/next_block.py R_NIGHT_1 --write

Stampa le prossime 40 battute con il testo inglese e scrive `blocks/block_NNN.json`
con le chiavi già pronte e i valori vuoti. Con `-n` si cambia la quantità.

Le battute escono nell'ordine in cui il gioco le conserva, che qui **è anche
l'ordine di lettura**: le chiavi vanno `LINE-1`, `LINE-2`, `LINE-5`, `LINE-5_2`,
`LINE-6`, `LINE-6a`… con le diramazioni come suffisso. È una fortuna che su
*Shadows of New York* non avevamo: lì la numerazione seguiva l'ordine di creazione
dei nodi nell'editor, e per ricostruire la lettura bisognava percorrere il grafo.

## 2. Tradurre

Riempire i valori in `blocks/block_NNN.json`. Un valore lasciato vuoto significa
"non ancora tradotta" e resta in inglese nel gioco, senza rompere niente.

Regole che valgono sempre:

- **La terminologia si consulta, non si inventa.** `reference/official_glossary.json`
  contiene la traduzione italiana ufficiale di *Coteries of New York*: `Kindred` è
  **Fratelli**, `Kine` è **vacche**, `Final Death` è **Morte Ultima**, `sire` resta
  **sire**.
- **Quello che il glossario non copre sta in `reference/supplementary_glossary.json`**,
  con la fonte dichiarata voce per voce. Ci sono finiti i nomi delle Discipline, che
  in *Coteries* non compaiono mai: `Animalism` è **Animalità**, `Obfuscate` è
  **Oscurazione**, `Presence` è **Ascendente**. Tre termini su cui l'intuito sbaglia,
  ed è successo davvero prima di verificarli. Ciò che è marcato `unverified` resta
  una scelta nostra, da rivedere.
- **I tag TMP vanno lasciati identici**, attributi compresi: `<link="Sire">` è ciò
  che apre la voce di glossario. Si traduce solo il testo fra i tag.
- **I segnaposto `{[...]}` non si toccano**: `{[button]}`, `{[NIGHT_NUMBER]}` sono
  sostituiti a runtime.
- **Le parentesi quadre si traducono** — `[DAUNT]`, `[Refuse.]` sono nomi di poteri
  e indicazioni di scena — ma devono restare **nello stesso numero**.

## 3. Controllare e fondere

    python tools/apply.py

Fonde tutti i blocchi in `translations/italian.json` e **si rifiuta di scrivere se
trova un errore**. Controlla:

| Controllo | Esito |
|---|---|
| chiave inesistente nel gioco | errore |
| stessa chiave tradotta due volte in modo diverso | errore |
| tag TMP o segnaposto alterati | errore |
| numero di parentesi quadre diverso | errore |
| battuta oltre 1,60x l'inglese | errore |
| battuta oltre 1,25x l'inglese | avviso |
| numero di a capo diverso | avviso |

Il controllo di lunghezza è più severo che su *Shadows* perché *Reckoning* è
**interamente doppiato**: un sottotitolo molto più lungo della battuta parlata esce
di sincronia. Si applica solo alle stringhe da 40 caratteri in su, perché su
un'etichetta di menu il rapporto non significa niente — *Credits* → *Titoli di
coda* è 2,00x ed è giusto così.

    python tools/apply.py --check     # controlla senza scrivere
    python tools/apply.py --report    # elenca le battute col rapporto peggiore

## 4. Provare in gioco

    bash tools/deploy.sh

Compila il plugin, installa DLL e traduzioni nella copia di lavoro e **verifica gli
md5**. Poi si avvia il gioco da `RoNY-game-copy`.

Il plugin rilegge `italian.json` a ogni avvio, quindi per provare una modifica alle
traduzioni **non serve ricompilare**: bastano `apply.py`, `deploy.sh` e un rilancio.

## Note

Le chiavi `Dialogue_0` e `Dialogue_1` contengono `"E!"` e `"Siema!"` — residui di
sviluppo in polacco. Le altre `Dialogue_N` sono invece testo vero.

Davanti a un comportamento strano in gioco, la prima mossa è mettere
`Enabled = false` nella config del plugin: il gioco gira intatto con la
diagnostica accesa, ed è così che abbiamo scoperto che il salvataggio delle
impostazioni e alcune eccezioni erano difetti suoi, non nostri.
