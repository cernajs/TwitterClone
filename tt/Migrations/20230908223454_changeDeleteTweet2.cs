using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tt.Migrations
{
    /// <inheritdoc />
    public partial class changeDeleteTweet2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Retweets_Tweets_TweetId",
                table: "Retweets");

            migrationBuilder.AddForeignKey(
                name: "FK_Retweets_Tweets_TweetId",
                table: "Retweets",
                column: "TweetId",
                principalTable: "Tweets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Retweets_Tweets_TweetId",
                table: "Retweets");

            migrationBuilder.AddForeignKey(
                name: "FK_Retweets_Tweets_TweetId",
                table: "Retweets",
                column: "TweetId",
                principalTable: "Tweets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
