﻿#language: de-DE
Funktionalität: Mohi - Validierung der gemeldeten Personen einer Datenmeldung

Szenario: Das Geburtsdatum darf nicht in der Zukunft liegen.
	Angenommen es ist ein 'MohiReport'
	Und die Eigenschaft 'birthday' von 'Person' ist auf '2058-04-30' gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''Geburtsdatum' von Klient 'Wolfgang Huber' darf nicht in der Zukunft liegen.'

#todo: > 1890, 1, 1 <heute
Szenario: Das Geburtsdatum darf nicht vor 1890 liegen.
	Angenommen es ist ein 'MohiReport'
	Und die Eigenschaft 'birthday' von 'Person' ist auf '1889-12-31' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Der Wert von 'Geburtsdatum' von Klient 'Wolfgang Huber' muss grösser oder gleich .*'

Szenario: PersonId ist nicht eindeutig.
	Angenommen der Id einer Mohi-Person ist nicht eindeutig
	Dann enthält das Validierungsergebnis den Fehler 'Der Id ist nicht eindeutig.'
#
Szenariogrundriss: Eine Eigenschaft ist nicht gesetzt
	Angenommen es ist ein 'MohiReport'
	Und die Eigenschaft '<Name>' von 'Person' ist nicht gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''<Bezeichnung>' von Klient '(.*)' darf nicht leer sein.'

Beispiele:
	| Name                      | Bezeichnung                                    |
	| family_name               | Familienname                                   |
	| given_name                | Vorname                                        |
	| birthday                  | Geburtsdatum                                   |
	| gender                    | Geschlecht                                     |
	| nationality               | Staatsbürgerschaft                             |
	| postcode                  | PLZ                                            |
	| city                      | Ort                                            |
	| care_allowance            | Pflegegeld                                     |
	| main_attendance_relation  | Verwandtschaftsverhältnis Hauptbetreuungspers. |
	| main_attendance_closeness | Räumliche Nähe Hauptbetreuungsperson           |
	
	# nicht vorhanden
	#| admission_type            | Aufnahmeart                          |
#
# Regex "^[a-zA-ZäöüÄÖÜß][-a-zA-ZäöüÄÖÜß ]*?[a-zA-ZäöüÄÖÜß]$"
Szenariogrundriss: Der Name einer Person enthält ein ungültiges Zeichen
	Angenommen es ist ein 'MohiReport'
	Und die Eigenschaft '<Name>' von 'Person' ist auf '<Wert>' gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''<Bezeichnung>' von Klient '(.*)' weist ein ungültiges Format auf.'

Beispiele:
	| Name        | Bezeichnung  | Wert |
	| family_name | Familienname | t@st |
	| given_name  | Vorname      | t@st |

# Länge 2 - 50 Zeichen
Szenariogrundriss: Der Familienname einer Person ist zu kurz / lang
	Angenommen es ist ein 'MohiReport'
	Und die Eigenschaft '<Name>' von 'Person' ist auf '<Wert>' gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''<Bezeichnung>' von Klient '(.*)' besitzt eine ungültige Länge'

Beispiele:
	| Name        | Bezeichnung  | Wert                                                     |
	| family_name | Familienname | abcdefghij abcdefghij abcdefghij abcdefghij abcdefghij x |
	| family_name | Familienname | x                                                        |

# Länge 2 - 30 Zeichen
Szenariogrundriss: Der Vorname einer Person ist zu kurz / lang
	Angenommen es ist ein 'MohiReport'
	Und die Eigenschaft '<Name>' von 'Person' ist auf '<Wert>' gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''<Bezeichnung>' von Klient '(.*)' besitzt eine ungültige Länge'

Beispiele:
	| Name       | Bezeichnung | Wert                               |
	| given_name | Vorname     | abcdefghij abcdefghij abcdefghij x |
	| given_name | Vorname     | x                                  |

# Staatsbürgerschaft
# ZZ ist unbekannt -> darum sollte ein Fehler kommen
Szenariogrundriss: Das Staatsbürgerschaft einer Person enthält einen ungültigen Wert
	Angenommen es ist ein 'MohiReport'
	Und die Eigenschaft '<Name>' von 'Person' ist auf '<Wert>' gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''<Bezeichnung>' von Klient 'Wolfgang Huber' hat einen ungültigen Wert'

Beispiele:
	| Name        | Bezeichnung        | Wert |
	| nationality | Staatsbürgerschaft | A    |


Szenariogrundriss: Das Staatsbürgerschaft einer Person enthält einen gültigen Wert
	Angenommen es ist ein 'MohiReport'
	Und die Eigenschaft '<Name>' von 'Person' ist auf '<Wert>' gesetzt
	Dann enthält das Validierungsergebnis keine Fehler

Beispiele:
	| Name        | Bezeichnung        | Wert |
	| nationality | Staatsbürgerschaft | AD   |
	| nationality | Staatsbürgerschaft | AT   |
	| nationality | Staatsbürgerschaft | ZZ   |

Szenariogrundriss: Die Datumsfelder dürfen keine Zeit enthalten
	Angenommen es ist ein 'MohiReport'
	Und die Datums-Eigenschaft '<Name>' von 'Person' hat eine Uhrzeit gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''<Bezeichnung>' von Klient 'Wolfgang Huber' darf keine Uhrzeit beinhalten.'

Beispiele:
	| Name     | Bezeichnung  |
	| birthday | Geburtsdatum |

# Ort / PLZ
Szenariogrundriss: Gültiger Ort / Plz
	Angenommen es ist ein 'MohiReport'
	Und die Eigenschaft 'postcode' von 'Person' ist auf '<PLZ>' gesetzt
	Und die Eigenschaft 'city' von 'Person' ist auf '<Ort>' gesetzt
	Dann enthält das Validierungsergebnis keine Fehler

Beispiele:
	| PLZ  | Ort       |
	| 6800 | Feldkirch |
	| 0000 | Anderer   |

Szenariogrundriss: Ungültiger Ort / Plz
	Angenommen es ist ein 'MohiReport'
	Und die Eigenschaft 'postcode' von 'Person' ist auf '<PLZ>' gesetzt
	Und die Eigenschaft 'city' von 'Person' ist auf '<Ort>' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Ungültige Kombination Ort/Plz bei Klient (.*)'

Beispiele:
	| PLZ  | Ort       |
	| 0349 | Feldkirch |
	| 6800 | xyz       |
