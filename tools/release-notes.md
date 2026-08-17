Traduzione italiana non ufficiale di **Vampire: The Masquerade — Reckoning of New York**.
Tutte le 11.141 battute del gioco: le otto notti, i tre finali alternativi, i dodici
intermezzi, il glossario, il diario, gli obiettivi e l'intera interfaccia.

## Non è ancora stata riletta da una persona

La traduzione è **completa**, ma non è ancora stata verificata da una rilettura umana
giocandoci. Per questo la versione resta sotto la 1.0: completo e verificato non sono la
stessa cosa. La 1.0 arriverà dopo una partita giocata per intero.

L'errore più probabile è **una frase rivolta a Kali con il genere sbagliato**: l'inglese
alla seconda persona non lo marca, quindi non si vede confrontando i testi, si vede solo
giocando. Se ne trovi uno, aprine una segnalazione qui.

Puoi anche correggerlo da solo: `BepInEx/plugins/italian.json` è un file di testo, si apre
con un editor qualsiasi e il gioco lo rilegge a ogni avvio, senza ricompilare niente.

## Cosa scaricare

### `RonyItalian-ita-v@VERSION@-with-bepinex.zip` — consigliato

Contiene tutto. Si scompatta nella cartella del gioco e basta: non serve installare né
configurare nient'altro. Include BepInEx 6 per IL2CPP nella build `6.0.0-be.785`, l'unica
con cui questa traduzione sia stata provata, già configurato per non aprire la finestra
nera della console.

### `RonyItalian-ita-v@VERSION@.zip` — la sola traduzione, 459 KB

Per chi ha già BepInEx e preferisce gestirselo. Contiene solo il nostro plugin e il testo
italiano.

**BepInEx non è incluso**, e senza non funziona: serve la versione **6 per IL2CPP a 64
bit**, dalle build di sviluppo su <https://builds.bepinex.dev/projects/bepinex_be> (il
file `BepInEx-Unity.IL2CPP-win-x64`).

> **Attenzione:** BepInEx 5, che è quello che si scarica per primo dal sito principale,
> con questo gioco **non funziona**. Deve essere la 6 per IL2CPP.

## Come si installa

Tutto va nella **cartella del gioco**, quella che contiene
`VtM Reckoning of New York.exe`, non in una sua sottocartella. Con Steam installato dove
capita di default è:

```
C:\Program Files (x86)\Steam\steamapps\common\Vampire The Masquerade - Reckoning of New York\
```

Se Steam è altrove: tasto destro sul gioco nella libreria → **Gestisci** → **Sfoglia file
locali**, e te la apre lui.

1. Scompatta lì l'archivio, lasciando che Windows unisca le cartelle quando lo chiede.
2. Avvia il gioco. Parte già in italiano.

A installazione fatta i nostri due file stanno qui — sono gli unici due di questo
progetto, tutto il resto è BepInEx:

```
Vampire The Masquerade - Reckoning of New York\
└── BepInEx\
    └── plugins\
        ├── RonyItalian.dll
        └── italian.json
```

Se hai preso l'archivio senza BepInEx, installa prima quello, nella stessa cartella.

**Il primo avvio è lento**: BepInEx deve generare gli assembly di interoperabilità del
gioco e ci mette una trentina di secondi, durante i quali la finestra sembra bloccata.
Dal secondo avvio in poi parte normalmente.

## Cosa non fa

**Non modifica nessun file del gioco.** La traduzione viene servita a runtime, mentre il
gioco chiede il testo. Questo vuol dire che la verifica dei file di Steam non ha niente da
ripristinare, e che disinstallare sono due cancellazioni: `RonyItalian.dll` e
`italian.json` da `BepInEx/plugins`.

Non contiene testo del gioco in inglese: nell'archivio viaggia solo l'italiano.

---

Traduzione amatoriale, senza alcun rapporto con Draw Distance né con Paradox Interactive.
Il gioco non è incluso: serve una copia regolare.
