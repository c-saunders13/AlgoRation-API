# Assessment Specification

## Instruction:

Please solve the problem in the most effective way possible, use this assessment to show case your skills as a developer, preferably use Angular and C# .net to complete this assessment.
Please provide a GitHub link or similar alternative to view the code on completion,
expected completion is 7 days from receiving the assessment.

Given the available ingredients shown and the list of recipes below, determine the optimal
combination of foods that can be created in order to feed as many people as possible.

## Ingredients:

Cucumber x 2
Olives x2
Lettuce x3
Meat x6
Tomato x6
Cheese x8
Dough x10

## Recipes

### Burger

Servings: 1

Meat x1
Lettuce x1
Tomato x1
Cheese x1
Dough x1

### Pie

Servings: 1

Dough x2
Meat x2

### Sandwich

Servings: 1

Dough x1
Cucumber x1

### Pasta

Servings: 2

Dough x2
Tomato x1
Cheese x2
Meat x1

### Salad

Servings: 3

Lettuce x2
Tomato x2
Cucumber x1
Cheese x2
Olives x1

### Pizza

Servings: 4

Dough x3
Tomato x2
Cheese x3
Olives x1

# AlgoRation API

## Implementation

The C# .NET Core Web API is meant to contain the logic of the whole application. I used the MVC architecture with the "V" aspect being contained in the Angular application, and I used the repository pattern for the data management aspect. As I mentioned in my interview, I do like the MVC architecture as it was something I taught and had to advocate for its use.

I also like the repository pattern because I've used it in multiple applications I've worked on, and I like that it provides the ability to swap out the database without having to rework a lot of the code as it provides a separation of concerns.

Right after reading the assessment question, the first implementation that came to my mind was to order the recipes by the most number of servings that can be made from them, descending, and then make as may as possible going down the list.

### Features

- MVC architecture
- Repository pattern
- Controllers for CRUDing ingredients and recipes, and for performing the rations calculation
- Seeded in-memory database for ease of testing and examination
- Ingredient foreign key constraints that prevent deleting an ingredient that is a FK in a recipe
- Basic test suite
- Validation on input

## Run, Test, and Build

### Prerequisites

- .NET SDK 10 installed

### Run the API

From the repository root:

```bash
dotnet run --project AlgoRationsProject/AlgoRationsAPI/AlgoRationsAPI.csproj
```

Default local URLs:

- http://localhost:5171
- https://localhost:7083

### Run tests

From the repository root:

```bash
dotnet test AlgoRationsProject/AlgoRationsAPI.Tests/AlgoRationsAPI.Tests.csproj --logger "console;verbosity=minimal"
```

### Build the application

Build the full solution from the repository root:

```bash
dotnet build metrofibre-assessment-rest-api.sln
```
