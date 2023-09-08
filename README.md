**Programmer documentation**

## Introduction

TwitterClone is project trying to immitate basic functionalities of Twitter. Its build using ASP.NET core MVC, EF Core and SQL server on back end. On front end it uses Razor pages with combination of jQuery for ajax calls and reloads without refreshing the page.
Features implemented are :
 
1. Register/Login using email and password
2. Each user can Create and Delete theier tweets
3. Tweets can be liked, reposted, bookmarked. On each Tweet can each user leave a commet. Commet is treated also as tweet.
4. Tweet can also contain tags starting with #. Each tag is clickable link that triggers searching all tweets containing that tag.
5. Search box implements multiple types of search based on the query.
6. In right panel there is list of currently trending tags and and random list of other users that currently logged in user can follow
7. User can follow other users
8. When we view our profile there is option to edit our current profile details
9. Users can also chat between each other in real time
10. In left panel there are tabs for displaying currently popular tweets, bookmarked tweets, notifications and for messages

## getting started

To run this project simply clone this project

`git clone https://github.com/cernajs/tt.git`

Install dependencies

(via NuGet packet manager)

Add migration

- for migration change connection string in appsettings.jsonn
- run `update-database` command with migration name,

#### Contribution Guidelines

-   **Code Review Process**: All code changes must be reviewed via pull requests.
-   **Adding functionality**: When adding functionality you can create new service or update new service. When Creating new service you also need to create new interface and add it to dependency injection in Program.cs. When adding functionality to existing service, first update already existing interface and then add functionality to specific implementation of service.
