import './markdown.scss';
import { parseMarkdown } from './md';

// function handleMentions(text: string) {
//   // Handle mentions: <@id>, @&id>, <#id>, <\:emoji\:id>, <a\:emoji\:id>, {everyone}, {here}
//   // Replace mentions with appropriate HTML tags
//   text = text.replace(/<@(\d+)>/gm, '<span class="mention">@user</span>'); // User mentions
//   text = text.replace(/<@&(\d+)>/gm, '<span class="mention">@role</span>'); // Role mentions
//   text = text.replace(/<#(\d+)>/gm, '<span class="mention">#channel</span>'); // Channel mentions

//   // Custom emojis
//   // Replace Discord emojis with <img> tags
//   text = text.replace(/<(a)?:[^:]+:(\d+)>/gm, (_match, animated, emojiId) => {
//     const extension = animated ? 'gif' : 'png'; // Determine the extension based on whether the emoji is animated
//     return `<img class="emoji" src="https://cdn.discordapp.com/emojis/${emojiId}.${extension}" alt="emoji"/>`;
//   });

//   text = text.replace(/\{everyone\}/gm, '<span class="mention">@everyone</span>'); // @everyone
//   text = text.replace(/\{here\}/gm, '<span class="mention">@here</span>'); // @here
//   return text;
// }

// function handleHeadings(text: string) {
//   // Handle headings: # heading, ## heading, ### heading
//   // Replace headings with appropriate HTML heading tags
//   text = text.replace(/^#\s+(.*)$/gm, '<h1>$1</h1>'); // Heading 1
//   text = text.replace(/^##\s+(.*)$/gm, '<h2>$1</h2>'); // Heading 2
//   text = text.replace(/^###\s+(.*)$/gm, '<h3>$1</h3>'); // Heading 3

//   return text;
// }

// function handleLists(text: string) {
//   // Handle lists
//   // Replace lists with appropriate HTML list tags
//   // TODO: fix this thing
//   // text = text.replace(/^(\s*)-\s+(.*)$/gm, (_match, spaces, content) => {
//   // const indentLevel = spaces.length / 2; // Each level of indentation represents two spaces
//   // let listTag = '';
//   // if (indentLevel === 0) {
//   //   listTag = '<ul>';
//   // } else if (indentLevel === 1 || indentLevel === 2) {
//   //   listTag = '<ul>'.repeat(indentLevel); // Generate opening <ul> tags based on the indent level
//   // } else {
//   //   listTag = '<ul>'.repeat(2); // Generate opening <ul> tags for subitems
//   //   listTag += '<ul>'.repeat(indentLevel - 2); // Generate opening <ul> tags for subsubitems
//   // }
//   // listTag += `<li>${content}</li>`;
//   // listTag += '</ul>'.repeat(indentLevel); // Generate closing </ul> tags based on the indent level
//   //   return content;
//   // });

//   return text;
// }

// function handleHyperlinks(text: string) {
//   // Handle hyperlinks
//   // Replace hyperlinks with appropriate HTML tags
//   text = text.replace(/\[([^\]]+)\]\((?:<([^>]+)>|([^)]+))\)/g, (match, text, link1, link2) => {
//     const link = link1 || link2;
//     // Check if link is a valid URL
//     const validUrl = /^(ftp|http|https):\/\/[^ "]+$/.test(link);
//     if (validUrl) {
//       return `<a href="${link}" target="_blank">${text}</a>`;
//     } else {
//       return match; // Return the original match if link is not a valid URL
//     }
//   });
//   return text;
// }

// function handleCodeBlocks(text: string) {
//   // Handle code blocks
//   // Replace code blocks with appropriate HTML tags
//   // TODO: implement code language support
//   return text;
// }

// function handleQuotes(text: string) {
//   // Handle quotes
//   // Replace quotes with appropriate HTML tags
//   text = text.replace(/^>([^>].*)$/gm, '<span class="quote">$1</span>'); // Single-line quotes
//   text = text.replace(/^>>>(.*)$/gm, '<span class="quote">$1</span>'); // Multi-line quotes
//   return text;
// }

// function handleBaseMarkdown(text: string) {
//   // Handle basic markdown formatting
//   // Replace markdown with appropriate HTML tags
//   // Handle bold: **text**
//   text = text.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');

//   // Handle underline: __text__
//   text = text.replace(/__(.*?)__/g, '<u>$1</u>');

//   // Handle italic: *text* and _text_
//   text = text.replace(/(?:\*|_)(.*?)(?:\*|_)/g, '<em>$1</em>');
  
//   return text;
// }

// function render(markdownText: string) {
//   markdownText = handleMentions(markdownText);
//   markdownText = handleHeadings(markdownText);
//   markdownText = handleLists(markdownText);
//   markdownText = handleHyperlinks(markdownText);
//   markdownText = handleCodeBlocks(markdownText);
//   markdownText = handleQuotes(markdownText);
//   markdownText = handleBaseMarkdown(markdownText);

//   return (
//     <div className="markdown-content" dangerouslySetInnerHTML={{ __html: markdownText }} />
//   );
// }

export default function DiscordMarkdown({ children }: { children: string }) {
  const md = parseMarkdown(children);
  return <div className='markdown-content'>{md}</div>;
}

