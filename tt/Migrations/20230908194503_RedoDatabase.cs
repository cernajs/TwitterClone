using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tt.Migrations
{
    /// <inheritdoc />
    public partial class RedoDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TweetHashtags_Hashtags_HashtagId",
                table: "TweetHashtags");

            migrationBuilder.DropForeignKey(
                name: "FK_TweetHashtags_Tweets_TweetId",
                table: "TweetHashtags");

            migrationBuilder.AddForeignKey(
                name: "FK_TweetHashtags_Hashtags_HashtagId",
                table: "TweetHashtags",
                column: "HashtagId",
                principalTable: "Hashtags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TweetHashtags_Tweets_TweetId",
                table: "TweetHashtags",
                column: "TweetId",
                principalTable: "Tweets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TweetHashtags_Hashtags_HashtagId",
                table: "TweetHashtags");

            migrationBuilder.DropForeignKey(
                name: "FK_TweetHashtags_Tweets_TweetId",
                table: "TweetHashtags");

            migrationBuilder.AddForeignKey(
                name: "FK_TweetHashtags_Hashtags_HashtagId",
                table: "TweetHashtags",
                column: "HashtagId",
                principalTable: "Hashtags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TweetHashtags_Tweets_TweetId",
                table: "TweetHashtags",
                column: "TweetId",
                principalTable: "Tweets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
