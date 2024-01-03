#language: de-DE
Funktionalität: Mkkp - Validierung der gemeldeten Personen einer Datenmeldung

Szenario: Das Geburtsdatum darf nicht in der Zukunft liegen.
	Angenommen es ist ein 'MkkpReport'
	Und die Eigenschaft 'birthday' von 'Person' ist auf '2058-04-30' gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''Geburtsdatum' von Klient 'Peter Gruber' darf nicht in der Zukunft liegen.'

#todo: >30 Jahre <heute
Szenario: Das Geburtsdatum darf nicht vor 1900 liegen.
	Angenommen es ist ein 'MkkpReport'
	Und die Eigenschaft 'birthday' von 'Person' ist auf '1899-12-31' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Der Wert von 'Geburtsdatum' von Klient 'Peter Gruber' muss grösser oder gleich .*'

Szenario: PersonId ist nicht eindeutig.
	Angenommen der Id einer Mkkp-Person ist nicht eindeutig
	Dann enthält das Validierungsergebnis den Fehler 'Die Id von Klient '1' ist nicht eindeutig.'

Szenariogrundriss: Eine Eigenschaft ist nicht gesetzt 1
	Angenommen es ist ein 'MkkpReport'
	Und die Eigenschaft '<Name>' von 'Person' ist nicht gesetzt
	Dann enthält das Validierungsergebnis genau einen Fehler
	Und enthält das escapte Validierungsergebnis den Fehler ''<Bezeichnung>' von Klient '<NewName>' darf nicht leer sein.'
Beispiele:
	| Name            | Bezeichnung                                 | NewName      |
	| birthday        | Geburtsdatum                                | Peter Gruber |
	| family_name     | Familienname                                | Peter        |
	| given_name      | Vorname                                     | Gruber       |
	| insurance       | Versicherung                                | Peter Gruber |
	| postcode        | Plz                                         | Peter Gruber |
	| city            | Ort                                         | Peter Gruber |
	| gender          | Geschlecht                                  | Peter Gruber |
	| referrer        | Zuweiser                                    | Peter Gruber |
	| hospital_doctor | Betreuender Arzt (Krankenhaus)              | Peter Gruber |
	| local_doctor    | Betreuender Arzt (Niedergelassener Bereich) | Peter Gruber |
 
Szenariogrundriss: Eine Eigenschaft vom Report mit einem ungültigen Wert gesetzt.
	Angenommen es ist ein 'MkkpReport'
	Und die Eigenschaft '<Name>' von 'Person' ist auf '<Wert>' gesetzt
	Dann enthält das Validierungsergebnis genau einen Fehler
    Und die Fehlermeldung lautet: 'Für '<Bezeichnung>'(.*)'<Wert>' kein gültiger Code.'
Beispiele:
	| Name      | Bezeichnung  | Wert |
	| insurance | Versicherung | test |

Szenariogrundriss: Der Name einer Person enthält ein ungültiges Zeichen
	Angenommen es ist ein 'MkkpReport'
	Und die Eigenschaft '<Name>' von 'Person' ist auf '<Wert>' gesetzt
	Dann enthält das escapte Validierungsergebnis den Fehler ''<Bezeichnung>' von Klient '<NewName>' weist ein ungültiges Format auf.'
Beispiele:
	| Name            | Bezeichnung                                 | Wert | NewName      |
	| family_name     | Familienname                                | t@st | Peter t@st   |
	| given_name      | Vorname                                     | t@st | t@st Gruber  |
	| hospital_doctor | Betreuender Arzt (Krankenhaus)              | t@st | Peter Gruber |
	| local_doctor    | Betreuender Arzt (Niedergelassener Bereich) | t@st | Peter Gruber |

	
Szenariogrundriss: Der Name einer Person enthält ein spezielles, aber gültiges Zeichen
	Angenommen es ist ein 'MkkpReport'
	Und die Eigenschaft '<Name>' von 'Person' ist auf '<Wert>' gesetzt
	Dann enthält das Validierungsergebnis keine Fehler
Beispiele:
	| Name            | Bezeichnung                                 | Wert          |
	| family_name     | Familienname                                | Chloé         |
	| given_name      | Vorname                                     | Raphaël       |
	| given_name      | Vorname                                     | Sr. Anna      |
	| hospital_doctor | Betreuender Arzt (Krankenhaus)              | Dr. Frank     |
	| local_doctor    | Betreuender Arzt (Niedergelassener Bereich) | Dr. Dr. Frank |


Szenariogrundriss: Die Datumsfelder dürfen keine Zeit enthalten
	Angenommen es ist ein 'MkkpReport'
	Und die Datums-Eigenschaft '<Name>' von 'Person' hat eine Uhrzeit gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''<Bezeichnung>' von Klient 'Peter Gruber' darf keine Uhrzeit beinhalten.'
Beispiele:
	| Name     | Bezeichnung  |
	| birthday | Geburtsdatum |
   
Szenario: Der Zuweiser ist 'Anderer', dann muss 'Anderer Zuweiser' befüllt sein
	Angenommen es ist ein 'MkkpReport'
	Und die Eigenschaft 'referrer' von 'Person' ist auf 'OtherReferrer' gesetzt
	Und die Eigenschaft 'other_referrer' von 'Person' ist nicht gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Wenn der Zuweiser von Klient 'Peter Gruber' ein Anderer Zuweiser ist, dann muss Anderer Zuweiser gesetzt sein.'

Szenario: Zuweiser ist undefiniert
	Angenommen es ist ein 'MkkpReport'
	Und die Eigenschaft 'referrer' von 'Person' ist auf 'UndefinedReferrer' gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''Zuweiser' von Klient 'Peter Gruber' darf nicht leer sein.'

Szenario: Geschlecht ist undefiniert
	Angenommen es ist ein 'MkkpReport'
	Und die Eigenschaft 'gender' von 'Person' ist auf 'UndefinedGe' gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''Geschlecht' von Klient 'Peter Gruber' darf nicht leer sein.'

Szenario: Pflegegeld ist undefiniert
	Angenommen es ist ein 'MkkpReport'
	Und die Eigenschaft 'care_allowance' von 'Person' ist auf 'UndefinedAllowance' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Pflegegeld' von Klient 'Peter Gruber' darf nicht leer sein.'

Szenario: Es dürfen keine doppelten Diagnosegruppen vorhanden sein.
	Angenommen es ist ein 'MkkpReport'
	Und die Mkkp-Diagnose(n) ist auf 'GeneticDisease, GeneticDisease' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Es dürfen keine doppelten Diagnosegruppen für Klient 'Peter Gruber' vorhanden sein.'

Szenario: Es muss mindestens eine Diagnosegruppe vorhanden sein
	Angenommen die Mkkp-Diagnose(n) ist auf '' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Es muss mindestens eine Diagnosegruppe für Klient 'Peter Gruber' vorhanden sein.'

Szenario: Es muss mindestens eine Diagnosegruppe vorhanden sein 2
	Angenommen die Mkkp-Diagnose(n) ist auf 'UndefinedDiagnosisGroup' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Es muss mindestens eine Diagnosegruppe für Klient 'Peter Gruber' vorhanden sein.'

Szenariogrundriss: Es darf nur eine Pallativ Diagnosegruppe vorhanden sein
	Angenommen die Mkkp-Diagnose(n) ist auf '<Wert>' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Es darf nur eine Palliativ Diagnose Gruppe für Klient 'Peter Gruber' vorhanden sein.'
Beispiele:
	| Wert                             |
	| PalliativeCare1, PalliativeCare2 |
	| PalliativeCare1, PalliativeCare3 |
	| PalliativeCare1, PalliativeCare4 |
	| PalliativeCare2, PalliativeCare3 |
	| PalliativeCare2, PalliativeCare4 |
	| PalliativeCare3, PalliativeCare4 |
    
Szenariogrundriss: Es dürfen mehrere Diagnosegruppen kombiniert sein
	Angenommen die Mkkp-Diagnose(n) ist auf '<Wert>' gesetzt
	Dann enthält das Validierungsergebnis keine Fehler
Beispiele:
	| Wert                                                                                                                              |
	| OncologicalDisease, Premature, MetabolicDisease, NeurologicalDisease, SurgicalCare, HeartDisease, GeneticDisease, PalliativeCare1 |
	| OncologicalDisease, Premature, MetabolicDisease, NeurologicalDisease, SurgicalCare, HeartDisease, GeneticDisease, PalliativeCare2 |
	| OncologicalDisease, Premature, MetabolicDisease, NeurologicalDisease, SurgicalCare, HeartDisease, GeneticDisease, PalliativeCare3 |
	| OncologicalDisease, Premature, MetabolicDisease, NeurologicalDisease, SurgicalCare, HeartDisease, GeneticDisease, PalliativeCare4 |


	# Ort / PLZ
Szenariogrundriss: Gultiger Ort / Plz
	Angenommen es ist ein 'CmReport'
	Und die Eigenschaft 'postcode' von 'Person' ist auf '<PLZ>' gesetzt
	Und die Eigenschaft 'city' von 'Person' ist auf '<Ort>' gesetzt
	Dann enthält das Validierungsergebnis keine Fehler

Beispiele:
	| PLZ  | Ort       |
	| 6800 | Feldkirch |
	| 0000 | Anderer   |

Szenariogrundriss: Ungültiger Ort / Plz
	Angenommen es ist ein 'CmReport'
	Und die Eigenschaft 'postcode' von 'Person' ist auf '<PLZ>' gesetzt
	Und die Eigenschaft 'city' von 'Person' ist auf '<Ort>' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Ungültige Kombination Ort/Plz bei Klient (.*)'

Beispiele:
	| PLZ  | Ort       |
	| 0349 | Feldkirch |
	| 6800 | xyz       |
