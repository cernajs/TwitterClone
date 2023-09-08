using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tt.Migrations
{
    /// <inheritdoc />
    public partial class changeDeleteTweet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TweetLikes_Tweets_TweetId",
                table: "TweetLikes");

            migrationBuilder.AddForeignKey(
                name: "FK_TweetLikes_Tweets_TweetId",
                table: "TweetLikes",
                column: "TweetId",
                principalTable: "Tweets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TweetLikes_Tweets_TweetId",
                table: "TweetLikes");

            migrationBuilder.AddForeignKey(
                name: "FK_TweetLikes_Tweets_TweetId",
                table: "TweetLikes",
                column: "TweetId",
                principalTable: "Tweets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
