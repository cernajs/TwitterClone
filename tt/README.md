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

Install dependencies

(via NuGet packet manager)

Add migration

- for migration change connection string in appsettings.json
- add migration using `add-migration {migrationName}`
- run `update-database` command with migration name,

## Getting Started
**Creating an Account** :

- Visit the homepage.
- Click on "Sign Up."
- Enter your email, and password.

**Logging In**:

- Click on "Log In."
- Enter your credentials and click "Submit."

**Navigating the Homepage** :

- Once logged in, you'll be directed to your home page where you can view your and other users tweets


## Features and Usage
**Posting a Tweet** :

- Click on text What's happening in home page and start typing
- Enter your tweet and click "Tweet"

**Following Users**:

- Search for users in the search bar.
- Click on their profile and click "Follow."

**Editing Profile**:

- Go to your profile by clicking your username at the top right.
- Click on "Edit Profile."
- You can edit your username and email here.

**Notifications**:

- Real-time notifications will appear when user you follow post tweet
