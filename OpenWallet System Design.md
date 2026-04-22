***
### Abstract
OpenWallet is an open source wallet management app that allows personal financing and money tracking

it's self-hostable, runs on docker with a web and mobile web friendly interface

### Technical Stack
- One app for the API and Web that runs on ASP.NET
- Uses .NET 10
- The database is postgresql latest version
- use swagger for API documentation
- user full client side blazor
- Add Docker file and docker compose to run both app and db, give a name for docker compose "OpenWallet"
- Use Database folder to have DB models and db context, migrations should be under this folder too

### Code Specs
- Don't use vars for types
- don't leave comments inside methods
- only leave documentation comments for API methods
- Keep things modular
- don't use repository pattern, use managers like accounts manager, records manager and so on
- Don't use Nullable Types feature in C#

### App Design

#### UI
- use sleek design, bootstrap
- use dark mode for the UI

#### Accounts
- The user can have as many accounts as he wants
- Support known currencies and gold of 24,21,18
- Accounts have these properties
	- Name
	- Currency
	- Initial Amount
	- Color
	  
#### Categories
- Each record must have an assigned category
- Categories can have sub categories 
- They have these properties
	- name
	- icon
	- color

#### Tags
- Each record can have as many tags as the user wants
- tags are basically a label that the user can use to mark the record as something, for example (2026 Trip)
- Tags have names

#### Records
- This is the most important part of the app and the most complicated
- A record is basically a transaction that can be made, it must use an account
- Records can be expense, income, or transfer
- Transfer Records are not a real type, but rather an easy way to create a record of transfer from one account and a record for the other account
- Records Have these properties:
	- Account
	- category or sub category
	- date and time
	- tags
	- notes
	- location (should be automatic from the mobile app)
	- attachments (as many files as the user want)

#### Templates
- Templates are reusable records, they can be stored as is, and when used, the user will be prompted with adding a new record predefined to the template
- Templates must have the same properties as records


#### Stores
- Stores are very similar to tags, but they can be part of records as a different way to track how much spent at a store

#### Debts
- debts is a way to mange lending and owing to people
- debts can be defined with an amount
- debts can have a list of related records to know how much is still owed but doesn't auto resolve
- the user can choose if he is lending or he owes someone
- the debt can have notes, and a name for the other party
- The user can close the debt as resolved

#### Stats and dashboards
- The home page of the app should show a dashboard
- an "eye" button that can mask all numbers for privacy
- First widget is accounts summary (standing amount)
- second widget shows last 10 records
- Third widget shows a pie chart of expenses by category (this month by default)
- it has another tab for expense by tags
- Fourth widget is balance trends, a chart of total balance trend
- Specific page for records that can be searched and filtered by every way possible