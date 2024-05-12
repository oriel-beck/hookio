import './markdown.scss';
import { parseMarkdown } from '../../simple-markdown';

export default function DiscordMarkdown({ children }: { children: string }) {
  const md = parseMarkdown(children);
  return <div className='markdown-content'>{md}</div>;
}

