﻿#language: de-DE
Funktionalität: StatLp - Validierung der Personen einer Datenmeldung

Szenario: Das Geburtsdatum darf nicht in der Zukunft liegen.
    Angenommen es ist ein 'StatLpReport'
    Und die Eigenschaft 'birthday' von 'Person' ist auf '2058-04-30' gesetzt
    Dann enthält das Validierungsergebnis den Fehler ''Geburtsdatum' darf nicht in der Zukunft liegen.'

Szenario: Das Geburtsdatum darf nicht vor 1890 liegen.
    Angenommen es ist ein 'StatLpReport'
    Und die Eigenschaft 'birthday' von 'Person' ist auf '1889-12-31' gesetzt
    Dann enthält das Validierungsergebnis den Fehler 'Der Wert von 'Geburtsdatum' muss grösser oder gleich .*'

Szenario: PersonId ist nicht eindeutig.
    Angenommen der Id einer Person ist nicht eindeutig
    Dann enthält das Validierungsergebnis den Fehler 'Der Id ist nicht eindeutig.'

Szenariogrundriss: Eine Eigenschaft ist nicht gesetzt
    Angenommen es ist ein 'StatLpReport'
    Und die Eigenschaft '<Name>' von 'Person' ist nicht gesetzt
    Dann enthält das Validierungsergebnis den Fehler ''<Bezeichnung>' darf nicht leer sein.'
Beispiele:
    | Name         | Bezeichnung    |
    | family_name  | Familienname   |
    | given_name   | Vorname        |
    | birthday     | Geburtsdatum   |
   
Szenariogrundriss: Der Name einer Person enthält ein ungültiges Zeichen
    Angenommen es ist ein 'StatLpReport'
    Und die Eigenschaft '<Name>' von 'Person' ist auf '<Wert>' gesetzt
    Dann enthält das escapte Validierungsergebnis den Fehler ''<Bezeichnung>' weist ein ungültiges Format auf.'
Beispiele: 
    | Name              | Bezeichnung    | Wert |
    | family_name       | Familienname   | t@st |
    | given_name        | Vorname        | t@st |

Szenariogrundriss: Der Familienname einer Person ist zu kurz / lang (Länge 2-50 Zeichen)
    Angenommen es ist ein 'StatLpReport'
    Und die Eigenschaft '<Name>' von 'Person' ist auf '<Wert>' gesetzt
    Dann enthält das Validierungsergebnis den Fehler ''<Bezeichnung>' von Klient '1' besitzt eine ungültige Länge'
Beispiele: 
    | Name              | Bezeichnung    | Wert                                                     |
    | family_name       | Familienname   | abcdefghij abcdefghij abcdefghij abcdefghij abcdefghij x |
    | family_name       | Familienname   | x                                                        |

Szenariogrundriss: Der Vorname einer Person ist zu kurz / lang ( Länge 2 - 30 Zeichen)
    Angenommen es ist ein 'StatLpReport'
    Und die Eigenschaft '<Name>' von 'Person' ist auf '<Wert>' gesetzt
    Dann enthält das Validierungsergebnis den Fehler ''<Bezeichnung>' von Klient '1' besitzt eine ungültige Länge'
Beispiele: 
    | Name             | Bezeichnung    | Wert                                |
    | given_name       | Vorname        | abcdefghij abcdefghij abcdefghij x  |
    | given_name       | Vorname        | x                                   |

Szenario: Die Liste enthält eine Person, die nicht in mindestens einem stay ist
    Angenommen es gibt eine weitere Person
    Dann enthält das Validierungsergebnis den Fehler 'Die Person '(.*)' wird in keinem Aufenthalt erwähnt.'

Szenario: Für eine Person wurde für unterschiedliche Aufenthalte ein unterschiedlicher Id verwendet.
    Angenommen mit den Stammdaten der StatLp-Person mit Id '1' gibt es einen weiteren Aufenthalt als Person mit Id '1X'
    Dann enthält das Validierungsergebnis den Fehler ''(.*)' wurde mehrmals mit folgenden Ids gemeldet: '(.*)'

Szenario: Für eine Person wurde für unterschiedliche Aufenthalte ein unterschiedlicher Id verwendet, dabei wurde ein alias-Id eingetragen.
    Angenommen mit den Stammdaten der StatLp-Person mit Id '1' gibt es einen weiteren Aufenthalt als Person mit Id '1X'
    Und es gibt einen Alias '1'='1X'
    Dann enthält das Validierungsergebnis keine Fehler
