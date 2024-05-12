/* eslint-disable @typescript-eslint/no-unused-vars */
import SimpleMarkdown, { SingleASTNode, State } from "./simple-markdown";

// region rules
const mentionRule = {
    order: SimpleMarkdown.defaultRules.text.order,

    match: function(source: string) {
        // Regular expression to match user mentions, role mentions, and channel mentions
        return /^<@&(.*?)>|^<@(.*?)>|^<#(.*?)>|^\{everyone\}|^\{here\}/.exec(source);
    },

    parse: function(capture: string[], parse: (str: string, state: State) => unknown, state: State) {
        // Extract the mention type and ID
        let type = '';
        let id = '';
        if (capture[1]) {
            type = 'role';
            id = capture[1];
        } else if (capture[2]) {
            type = 'user';
            id = capture[2];
        } else if (capture[3]) {
            type = 'channel';
            id = capture[3];
        } else if (capture[0] === "{everyone}") {
            type = 'everyone',
            id = "everyone"
        } else if (capture[0] === "{here}") {
            type = 'here',
            id = "here"
        }

        return {
            content: parse(id, state),
            mentionType: type
        };
    },

    react: function(node: SingleASTNode, _output: (node: SingleASTNode, state: State) => string, state: State) {

        // Return a span element with the appropriate class
        return SimpleMarkdown.reactElement("span", state.key!.toString(), {
            href: SimpleMarkdown.sanitizeUrl(node.target),
            title: node.title,
            className: 'mention',
            children: `@${node.mentionType}`
        });
    }
};

const emojiRule = {
    order: SimpleMarkdown.defaultRules.text.order,

    match: function(source: string) {
        return /^<(a?):([a-zA-Z0-9_]+):(\d{17,19})>/.exec(source);
    },

    parse: function(capture: string[], _parse: (str: string, state: State) => unknown, _state: State) {
        const isAnimated = capture[1] === 'a';
        const name = capture[2];
        const id = capture[3];
        const extension = isAnimated ? 'gif' : 'png';
        const url = `https://cdn.discordapp.com/emojis/${id}.${extension}`;
        
        return {
            name: name,
            url: url
        };
    },

    react: function(node: SingleASTNode, _output: (node: SingleASTNode, state: State) => string, state: State) {
        // Return an img element with the appropriate class and src
        return SimpleMarkdown.reactElement(
            'img',
            state.key!.toString(),
            {
                className: 'emoji',
                src: node.url,
                alt: node.name
            }
        );
    }
};

const rules = Object.assign({}, {
    Array: SimpleMarkdown.defaultRules.Array,
    underline: SimpleMarkdown.defaultRules.u,
    list: SimpleMarkdown.defaultRules.list,
    codeBlock: SimpleMarkdown.defaultRules.codeBlock,
    escape: SimpleMarkdown.defaultRules.escape,
    em: SimpleMarkdown.defaultRules.em,
    strong: SimpleMarkdown.defaultRules.strong,
    text: SimpleMarkdown.defaultRules.text,
    paragraph: SimpleMarkdown.defaultRules.paragraph,
    mentions: mentionRule,
    emojis: emojiRule
});

// region parser
// @ts-expect-error aaaa
const rawBuiltParser = SimpleMarkdown.parserFor(rules);
const parse = function (source: string) {
    const blockSource = source + "\n\n";
    return rawBuiltParser(blockSource, { inline: false });
};

const reactOutput = SimpleMarkdown.outputFor(rules, 'react');

export function parseMarkdown(str: string) {
    const syntaxTree = parse(str);
    return reactOutput(syntaxTree);
}
