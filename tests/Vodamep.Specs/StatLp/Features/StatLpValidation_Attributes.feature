#language: de-DE
Funktionalität: StatLp - Validierung Attribute einer Datenmeldung

Szenario: Ein Attribut enthält eine Person, die nicht in der Personenliste ist
	Angenommen es ist ein 'StatLpReport'
	Und die Eigenschaft 'person_id' von 'Attribute' ist auf '2' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'Person '2' ist nicht in der Personenliste vorhanden.'

Szenario: Ein Attribut muss im Meldezeitraum liegen
	Angenommen es ist ein 'StatLpReport'
	Und die Eigenschaft 'from' von 'Attribute' ist auf '2000-01-01' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'muss im Meldezeitraum liegen.'

Szenario: Von darf keine Zeit beinhalten
	Angenommen es ist ein 'StatLpReport'
	Und die Datums-Eigenschaft 'from' von 'Attribute' hat eine Uhrzeit gesetzt
	Dann enthält das Validierungsergebnis den Fehler ''Von' darf keine Uhrzeit beinhalten.'

# Todo => der Validator bekommt ein Enum (von Gerhard geliefert) im Konstruktor, hier abfragen
# Attribute CAREALLOWANCEARGE darf nur L0_AR sein, wenn es sich um Altersheim handelt
# Klären: Wie kommen wir zur Gruppe ABAH?
# - innerhalb von Vodamep lokal
# - innerhalb vom Vodamep Server
# - innerhalb der Connexia Logik

Szenario: Ein Attribut hat keinen Typ
	Angenommen es gibt am '2021-03-01' ein zusätzliches Attribut vom Typ 'Something' und dem Wert 'Unspecified'
	Dann enthält das Validierungsergebnis den Fehler 'Ein Merkmal von Klient '(.*)' darf nicht leer sein'
	
Szenario: Pflegegeld ist undefiniert
	Angenommen es gibt am '2021-03-01' ein zusätzliches Attribut vom Typ 'CareAllowance' und dem Wert 'Unspecified'
	Dann enthält das Validierungsergebnis den Fehler 'Für die Person '(.*)' wurde am (.*) kein Wert bei'
	
Szenario: Pflegstufe Arge ist undefiniert
	Angenommen es gibt am '2021-03-01' ein zusätzliches Attribut vom Typ 'CareAllowanceArge' und dem Wert 'Unspecified'
	Dann enthält das Validierungsergebnis den Fehler 'Für die Person '(.*)' wurde am (.*) kein Wert bei'
	
Szenario: Finanzierung ist undefiniert
	Angenommen es gibt am '2021-03-01' ein zusätzliches Attribut vom Typ 'Finance' und dem Wert 'Unspecified'
	Dann enthält das Validierungsergebnis den Fehler 'Für die Person '(.*)' wurde am (.*) kein Wert bei'
	
Szenariogrundriss: Fehlende Pflichtfelder für die Aufnahme
	Angenommen es ist ein 'StatLpReport'
    Und das Attribut '<Name>' fehlt
    Dann enthält das Validierungsergebnis den Fehler 'Für die Person '(.*)' wurde am (.*) keine '<Bezeichnung>' gemeldet.'
Beispiele:
    | Name              | Bezeichnung        |    
    | CareAllowance     | Pflegestufe        |
    | CareAllowanceArge | Pflegestufe (Arge) |
    | Finance           | Finanzierung       |

Szenario: Mehrfache Attribute	
    Angenommen es gibt am '2021-02-15' ein zusätzliches Attribut vom Typ 'CareAllowance' und dem Wert 'L2'
	Und es gibt am '2021-02-15' ein zusätzliches Attribut vom Typ 'CareAllowance' und dem Wert 'L3'
    Dann enthält das Validierungsergebnis den Fehler 'Beim Klient '(.*)' wurde am 15.02.2021 das Attribut 'Pflegestufe' mehrfach angegeben.'

Szenario: Ein Attribut wurde außerhalb eines Aufenthaltes gesetzt
	Angenommen die erste Aufnahme startet am '2021-05-30', dauert 30 Tage und ist eine 'ContinuousAt'
	Und die Eigenschaft 'from' von 'Attribute' ist auf '2021-01-01' gesetzt
	Dann enthält das Validierungsergebnis den Fehler 'wurde am (.*) das Attribut '(.*)' außerhalb eines Aufenthaltes angegeben'
