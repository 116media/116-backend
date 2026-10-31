using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Indexes every weight slot under both the French and the English configurations.
    /// The catalogue carries both languages, and French stemming leaves English words
    /// unstemmed, so a reader searching "record" never reached an article about a
    /// "recording". Replacing the generated column recomputes every row.
    /// </summary>
    public partial class AddBilingualSearchVector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dropping the column takes its index with it, so both are rebuilt below.
            migrationBuilder.Sql("ALTER TABLE content.search_documents DROP COLUMN search_vector;");

            migrationBuilder.Sql(
                """
                ALTER TABLE content.search_documents
                    ADD COLUMN search_vector tsvector
                    GENERATED ALWAYS AS (
                        setweight(to_tsvector('french',  content.immutable_unaccent(coalesce(title, ''))), 'A') ||
                        setweight(to_tsvector('english', content.immutable_unaccent(coalesce(title, ''))), 'A') ||
                        setweight(to_tsvector('french',  content.immutable_unaccent(coalesce(summary, ''))), 'B') ||
                        setweight(to_tsvector('english', content.immutable_unaccent(coalesce(summary, ''))), 'B') ||
                        setweight(to_tsvector('french',  content.immutable_unaccent(coalesce(keywords, ''))), 'C') ||
                        setweight(to_tsvector('english', content.immutable_unaccent(coalesce(keywords, ''))), 'C') ||
                        setweight(to_tsvector('french',  content.immutable_unaccent(coalesce(body, ''))), 'D') ||
                        setweight(to_tsvector('english', content.immutable_unaccent(coalesce(body, ''))), 'D')
                    ) STORED;
                """
            );

            migrationBuilder.Sql(
                """
                CREATE INDEX ix_search_documents_vector_visible
                    ON content.search_documents USING GIN (search_vector)
                    WHERE is_visible;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE content.search_documents DROP COLUMN search_vector;");

            migrationBuilder.Sql(
                """
                ALTER TABLE content.search_documents
                    ADD COLUMN search_vector tsvector
                    GENERATED ALWAYS AS (
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(title, ''))), 'A') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(summary, ''))), 'B') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(keywords, ''))), 'C') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(body, ''))), 'D')
                    ) STORED;
                """
            );

            migrationBuilder.Sql(
                """
                CREATE INDEX ix_search_documents_vector_visible
                    ON content.search_documents USING GIN (search_vector)
                    WHERE is_visible;
                """
            );
        }
    }
}
