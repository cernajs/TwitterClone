## Contribution Guidelines
-   **Code Review Process**: All code changes must be reviewed via pull requests.
-   **Adding functionality**: When adding functionality you can create new service or update new service. When Creating new service you also need to create new interface and add it to dependency injection in Program.cs. When adding functionality to existing service, first update already existing interface and then add functionality to specific implementation of service.

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

-   **async Task< IActionResult > Index (string id)** - Get user profile page with related data.
    -   Input: User ID as string
    -   Output: ViewResult with model of type `ApplicationUser`
 	
- **async Task< IActionResult > Follow (string userIdToFollow)** : create new follow relationship in database if it already doesnt exist
    -   Input: ID of user to follow as string
    -   Output: Redirect to profile from which request came
 
- **async Task< IActionResult > Unfollow (string userIdToUnfollow)** : remove follow relationship from database if it exists
    -   Input: User ID as string, followers or followings string as type
    -   Output: Redirect to profile from which request came
 
- **async Task< IActionResult > ShowUsers (string id, string type)** : Show all users that the current user is following or show all user that follows the current user based on type parameter.
    -   Input: User ID as string, followers or followings string as type
    -   Output: ViewResult with model of type `List<ApplicationUser>`
 
- **async Task< IActionResult > EditProfile ([FromBody] EditProfileViewModel model)** : Edit currently logged in user profile.
    -   Input: `EditProfileViewModel` object
    -   Output: JSON object indicating success or failure
 
- **async Task< IActionResult > ShowBookmarks ()** : Show all tweets that the current user has bookmarked.
    - Output : ViewResult with model of type `List<Tweet>`
 



**TweetController**

- **async Task< IActionResult > Create (string username, string tweet)** : Create a new tweet and notify followers of the user also parse hashtags and create them if they dont exist.
    -   Input: userame and tweet string
    -   Output: JSON object indicating success or failure
 
- **async Task< IActionResult > Delete (int tweetId)** : Delete a tweet if the user is the owner of the tweet.
    -   Input: userame and tweet string
    -   Output: Redirect to place of request origin
 
- **async Task< IActionResult > Like (int tweetId)** : Like a tweet by adding a TweetLike relationship to the database.
    -   Input: tweetId indicating which tweet to like
    -   Output: JSON object indicating success or failure
 
- **async Task< IActionResult > Unlike (int tweetId)** : Unlike a tweet by removing the TweetLike relationship from the database.
    -   Input: tweetId indicating which tweet to like
    -   Output: JSON object indicating success or failure
 
- **async Task< IActionResult > ShowLikes (int id)** : Show all users that have liked a tweet.
    -   Input: Tweet id for which we want to show likes
    -   Output: ViewResult with model of type `ApplicationUser`
 
- **async Task< IActionResult > Reply (int ParentTweetId, string Content)** : Reply to a tweet by creating a new tweet with the parent tweet id = ParentTweetId.
    -   Input: Tweet id to which we are replying to, string content of reply
    -   Output: Redirect to place of request origin
 
- **async Task< IActionResult > ViewReplies (int id)** : Show all replies to a tweet.
    -   Input: Tweet id for which we want to show replies
    -   Output: ViewResult with model of type `(Tweet, IEnumerable<Tweet>)`
 
- **async Task< IActionResult > Bookmark (int tweetId, bool isBookmarked)** : Bookmark a tweet by adding a TweetBookmark relationship to the database.
    -   Input: tweetId indicating which tweet to bookmarrk, bool isBookmarked indicating whether to bookmark/unbookmark
    -   Output: JSON object indicating success or failure

- **async Task< IActionResult > Retweet (int tweetId, bool isRetweet)** : Retweet a tweet by adding a TweetRetweet relationship to the database.
    -   Input: tweetId indicating which tweet to retweet, bool isRetweet indicating whether to reply/unreply
    -   Output: JSON object indicating success or failure



**HomeController**

- **async Task< IActionResult > Index ()** : Retrieve tweets for home page given the strategy.
     -   Output: ViewResult with model of type `IEnumerable<Tweet>`
 
- **async Task< IActionResult > Search (string searchQuery)** : Search for tweets by username or hashtag depending on the search query.
    -   Input: string searchQuery representing what we searchinng for
    -   Output: ViewResult with model of type `IEnumerable<Tweet>`
 
- **async Task< IActionResult > Popular ()** : Retrieve tweets for popular page given the strategy.
     -   Output: ViewResult with model of type `IEnumerable<Tweet>`
 
- **async Task< IActionResult > ShowNotifications ()** : Show notifications for the current user.
     -   Output: ViewResult with model of type `List<Notification>`
 


**ChatController**

- **async Task< IActionResult > Index ()** : Returns a view with all chats of the current user.
    -   Output: ViewResult with model of type `List<ChatViewModel>`
 
- **async Task< IActionResult > ChatWithSpecificUserAsync (string id)** : Return a view with all messages between the current user and the user with the id.
    -   Input: string User ID that current user want to chat with
    -   Output: ViewResult with model of type `List<SpecificChatViewModel>`
 
- **async Task< IActionResult > CreateMessage ([FromBody] ChatMessageDto chatMessageDto)** : Create a message between currentUser and recipient with content located in chatMessageDto and add it to the database.
    -   Input: JSON of new chat message
    -   Output: JSON object indicating success or failure
 


***ApiController**

- **async Task< IActionResult > GetNotificationCount ()** : Update the notification count for the current user when there is new notification.
    -   Output: JSON object with number of notifications count


- **async Task< IActionResult > GetTrendingTopics ()** : return the top 3 trending topics based on implementation of IHomeService
    -   Output: JSON object with top 3 most trending topics
 
- **async Task< IActionResult > GetFollowSuggest ()** : return 3 follow suggestions based on implementation of IHomeService
    -   Output: JSON object with 3 random users to follow
 
