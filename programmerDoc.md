## Codebase Overview
**Controllers :**
There are several controllers inn this application, namely :
- ApiController
- ChatController
- HomeController
- TweetController
- UserController

**Controllers**:
Each controller serves mainly as handler of HTTP requests. Each Controller is responsible for handling invalid inputs. Further actions with controller input are then delegated to services. Each service then has multiple methods, each rosponsible for retrieving specifc data to controller. Controller then check data retieved by service and if they are valid, they are passed to view.

**Models**:
Models represent individual tables in databse. There are models representing individual entities for example : User, Tweet, Tag. There are also models represening binding relationship in many to many relationships in database, for example : TweetLike or UserFollower.

**Services**:
Services separate bussiness logic from Controllers so when we need to change it we dont need to change controller at all. Each Service has its own Interface. 

**Tests**:
Tests are currently implemented in different project in different repository, see 
...

**Database Context**:
Database context for this app is named : TwitterContext. It defines tables in database and relationships between them.

**Views**:
For frontend we mainly use Razor views with a bit of javasript for SignalR connection and real time page updates without reload.



## API Documentation

**UserController**

-   **Index(string id)**: Display profile based on user id
    -   Input: User ID as string
    -   Output: ViewResult with model of type `ApplicationUser`
-   **EditProfile(EditProfileViewModel model)**: Allows user to edit their profile
    -   Input: `EditProfileViewModel` object
    -   Output: JSON object indicating success or failure

**UserService**

-   **EditUserProfileAsync(EditProfileViewModel model)**: Logic for updating user profile
    -   Input: `EditProfileViewModel` object
    -   Output: `EditProfileResult` object indicating success or failure and errors
