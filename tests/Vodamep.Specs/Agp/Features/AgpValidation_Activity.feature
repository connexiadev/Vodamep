﻿#language: de-DE
Funktionalität: Agp - Validierung der gemeldeten Aktivitäten der Datenmeldung

Szenario: Pesonen ID ist nicht gesetzt
	Angenommen es ist ein 'AgpReport'
	Und die Eigenschaft 'person_id' von 'Activity' ist nicht gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''Personen-ID' von Aktivität darf nicht leer sein.'

Szenariogrundriss: Eine Eigenschaft ist nicht gesetzt
	Angenommen es ist ein 'AgpReport'
	Und die Eigenschaft '<Name>' von 'Activity' ist nicht gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''<Bezeichnung>' von Aktivität von Klient '(.*)' darf nicht leer sein.'
Beispiele:
	| Name | Bezeichnung |
	| date | Datum       |
	| id   | ID          |


Szenario: Einsatzort ist undefiniert
	Angenommen es ist ein 'AgpReport'
	Und die Eigenschaft 'place_of_Action' von 'Activity' ist auf 'UndefinedPlace' gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''Einsatzort' von Aktivität von Klient '(.*)' darf nicht leer sein.'



Szenario: Leistungszeit muss > 0 sein
	Angenommen es ist ein 'AgpReport'
	Angenommen die Eigenschaft 'minutes' von 'StaffActivity' ist nicht gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Leistungszeit mit Id '(.*)' muss größer 0 sein.'

Szenariogrundriss: Minuten Werte müssen > 0 sein
	Angenommen es ist ein 'AgpReport'
	Und die Eigenschaft 'minutes' von 'Activity' ist auf '<Wert>' gesetzt
	Dann enthält das Validierungsergebnis genau einen Fehler
	Und die Fehlermeldung lautet: 'Leistungszeit von Klient '(.*)' muss größer 0 sein.'
Beispiele:
	| Wert |
	| 0    |
	| -1   |

Szenariogrundriss: Minuten  dürfen nur in 5 Minuten Schritten eingegeben werden
	Angenommen es ist ein 'AgpReport'
	Und die Eigenschaft 'minutes' von 'Activity' ist auf '<Wert>' gesetzt
	Dann enthält das Validierungsergebnis genau einen Fehler
	Und die Fehlermeldung lautet: 'Leistungszeit von '(.*)' darf nur in 5 Minuten Schritten eingegeben werden.'
Beispiele:
	| Wert |
	| 1    |
	| 3    |
	| 17   |

Szenariogrundriss: Minuten  dürfen nur in 5 Minuten Schritten eingegeben werden - korrekt
	Angenommen es ist ein 'AgpReport'
	Und die Eigenschaft 'minutes' von 'Activity' ist auf '<Wert>' gesetzt
	Dann enthält das Validierungsergebnis keine Fehler
	Und es enthält keine Warnungen
Beispiele:
	| Wert |
	| 5    |
	| 10   |
	| 15   |


Szenario: Mehrfache Leistungen pro Klient pro Tag
	Angenommen es werden zusätzliche Leistungen pro AGP-Klient an einem Tag eingetragen
	Dann enthält das Validierungsergebnis keine Fehler
	Und es enthält keine Warnungen

Szenario: Mehrfache Leistungstypen pro Leistung
	Angenommen die Leistungstypen 'ClearingAt,CareDocumentationAt' sind für eine AGP-Aktivität gesetzt
	Dann enthält das Validierungsergebnis keine Fehler
	Und es enthält keine Warnungen

Szenario: Doppelte Leistungen innerhalb einer Aktivität
	Angenommen die Leistungstypen 'CareDocumentationAt,CareDocumentationAt' sind für eine AGP-Aktivität gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Innerhalb einer Aktivität von Klient 'Peter Gruber' dürfen keine doppelten Leistungstypen vorhanden sein.'

Szenario: Es muss mindestens ein Leistungstyp pro Leistung vorhanden sein
	Angenommen die Leistungstypen '' sind für eine AGP-Aktivität gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Leistungsbereiche' von Aktivität von Klient 'Peter Gruber' darf nicht leer sein.'

Szenario: Eine Aktivität ist nach dem Meldungszeitraum.
	Angenommen es ist ein 'AgpReport'
	Und die Eigenschaft 'date' von 'Activity' ist auf '2058-09-29' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Datum' der Aktivität (.*) muss innerhalb des Meldungszeitraums liegen.'
    
Szenario: Eine Aktivität ist vor dem Meldungszeitraum.
	Angenommen es ist ein 'AgpReport'
	Und die Eigenschaft 'date' von 'Activity' ist auf '2008-04-30' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Datum' der Aktivität (.*) muss innerhalb des Meldungszeitraums liegen.'

Szenario: Eine Aktivität ohne entsprechenden Eintrag in Persons
	Angenommen es ist ein 'AgpReport'
	Und die Eigenschaft 'person_id' von 'Activity' ist auf '-1' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Eine Aktivität vom (.*) mit der ID (.*) ist keiner vorhandenen Person'

Szenario: Eine Aktivität ist nach dem Meldungszeitraum - Mitarbeiter Leistungen
	Angenommen es ist ein 'AgpReport'
	Und die Eigenschaft 'date' von 'Activity' ist auf '2058-09-29' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Datum' der Aktivität (.*) muss innerhalb des Meldungszeitraums liegen.'
    
Szenario: Eine Aktivität ist vor dem Meldungszeitraum - Mitarbeiter Leistungen
	Angenommen es ist ein 'AgpReport'
	Und die Eigenschaft 'date' von 'Activity' ist auf '2008-04-30' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Datum' der Aktivität (.*) muss innerhalb des Meldungszeitraums liegen.'

Szenario: Eine Person ohne Aktivität.
	Angenommen zu einer AGP-Person sind keine AGP-Aktivitäten dokumentiert
	Dann enthält das Validierungsergebnis den Fehler 'Keine Aktivitäten'





