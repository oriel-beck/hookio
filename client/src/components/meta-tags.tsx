import { Helmet } from "react-helmet";

export default function MetaTags() {
    return (
        <Helmet>
            {/* Title Tag */}
            <title>Hookio</title>

            {/* Description Tag */}
            <meta name="description" content="Hookio.gg is a site that uses Discord Webhooks to announce YouTube videos being released and updated and Twitch streams starting, updating or ending. The site allows you to fully customize the message and the webhook that sends it using an embed builder." />

            {/* Keywords Tag */}
            <meta name="keywords" content="Discord webhooks, youtube notifications, twitch notifications, discord embed builder, embed builder" />

            {/* Viewport Tag */}
            <meta name="viewport" content="width=device-width, initial-scale=1" />

            {/* Canonical Tag */}
            <link rel="canonical" href="https://hookio.gg" />

            {/* Open Graph Tags */}
            <meta property="og:title" content="Hookio" />
            <meta property="og:description" content="Hookio.gg is a site that uses Discord Webhooks to announce YouTube videos being released and updated and Twitch streams starting, updating or ending. The site allows you to fully customize the message and the webhook that sends it using an embed builder." />
            <meta property="og:url" content="https://hookio.gg" />

            {/* Twitter Card Tags */}
            <meta name="twitter:title" content="Hookio" />
            <meta name="twitter:description" content="Hookio.gg is a site that uses Discord Webhooks to announce YouTube videos being released and updated and Twitch streams starting, updating or ending. The site allows you to fully customize the message and the webhook that sends it using an embed builder." />
            <meta name="twitter:card" content="summary" />

            {/* Structured Data Markup */}
            {/* Example of Article Markup */}
            {/* Replace 'YourArticle' with appropriate schema */}
            <script type="application/ld+json">{`
          {
            "@context": "https://schema.org",
            "@type": "Article",
            "headline": "Hookio",
            "description": "Hookio.gg is a site that uses Discord Webhooks to announce YouTube videos being released and updated and Twitch streams starting, updating or ending. The site allows you to fully customize the message and the webhook that sends it using an embed builder.",
            "publisher": {
              "@type": "Organization",
              "name": "Hookio",
              "logo": {
                "@type": "ImageObject",
                "url": "https://hookio.gg/logo.png"
              }
            },
            "url": "https://hookio.gg"
          }
        `}</script>
        </Helmet>
    );
}