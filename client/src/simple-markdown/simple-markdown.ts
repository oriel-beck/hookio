/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-explicit-any */
/**
 * Simple-Markdown
 * ===============
 *
 * Simple-Markdown's primary goal is to be easy to adapt. It aims
 * to be compliant with John Gruber's [Markdown Syntax page][1],
 * but compatiblity with other markdown implementations' edge-cases
 * will be sacrificed where it conflicts with simplicity or
 * extensibility.
 *
 * If your goal is to simply embed a standard markdown implementation
 * in your website, simple-markdown is probably not the best library
 * for you (although it should work). But if you have struggled to
 * customize an existing library to meet your needs, simple-markdown
 * might be able to help.
 *
 * Many of the regexes and original logic has been adapted from
 * the wonderful [marked.js](https://github.com/chjj/marked)
 */
import type { Capture, MatchFunction, State } from "./troublesome-types"
import type * as React from "react";

// export { libVersion } from "./version";

// Type Definitions:

type Attr = string | number | boolean | null | undefined;

type SingleASTNode = {
    type: string;
    [key: string]: any;
};

type UnTypedASTNode = {
    [key: string]: any;
};

type ASTNode = SingleASTNode | Array<SingleASTNode> | UnTypedASTNode | UnTypedASTNode[];

type ReactElement = React.ReactElement<any>;
type ReactElements = React.ReactNode;

type Parser = (
    source: string,
    state?: State | null | undefined,
) => Array<SingleASTNode | UnTypedASTNode>;

type ParseFunction = (
    capture: Capture,
    nestedParse: Parser,
    state: State,
) => UnTypedASTNode | ASTNode;

type SingleNodeParseFunction = (
    capture: Capture,
    nestedParse: Parser,
    state: State,
) => UnTypedASTNode;

type Output<Result> = (
    node: ASTNode | UnTypedASTNode,
    state?: State | null | undefined,
) => Result;

type NodeOutput<Result> = (
    node: SingleASTNode | UnTypedASTNode,
    nestedOutput: Output<Result>,
    state: State,
) => Result;

type ArrayNodeOutput<Result> = (
    node: Array<SingleASTNode | UnTypedASTNode>,
    nestedOutput: Output<Result>,
    state: State,
) => Result;

type ReactOutput = Output<ReactElements>;
type ReactNodeOutput = NodeOutput<ReactElements>;
type HtmlOutput = Output<string>;
type HtmlNodeOutput = NodeOutput<string>;

type ParserRule = {
    readonly order: number;
    readonly match: MatchFunction;
    readonly quality?: (
        capture: Capture,
        state: State,
        prevCapture: string,
    ) => number;
    readonly parse: ParseFunction;
};

type SingleNodeParserRule = {
    readonly order: number;
    readonly match: MatchFunction;
    readonly quality?: (
        capture: Capture,
        state: State,
        prevCapture: string,
    ) => number;
    readonly parse: SingleNodeParseFunction;
};

type ReactOutputRule = {
    // we allow null because some rules are never output results, and that's
    // legal as long as no parsers return an AST node matching that rule.
    // We don't use ? because this makes it be explicitly defined as either
    // a valid function or null, so it can't be forgotten.
    readonly react: ReactNodeOutput | null;
};

type HtmlOutputRule = {
    readonly html: HtmlNodeOutput | null;
};

type ArrayRule = {
    // @ts-expect-error - TS2411 - Property 'react' of type 'ArrayNodeOutput<ReactNode> | undefined' is not assignable to 'string' index type 'ArrayNodeOutput<any>'.
    readonly react?: ArrayNodeOutput<ReactElements>;
    // @ts-expect-error - TS2411 - Property 'html' of type 'ArrayNodeOutput<string> | undefined' is not assignable to 'string' index type 'ArrayNodeOutput<any>'.
    readonly html?: ArrayNodeOutput<string>;
    readonly [key: string]: ArrayNodeOutput<any>;
};

type ParserRules = {
    // @ts-expect-error - TS2411 - Property 'Array' of type 'ArrayRule | undefined' is not assignable to 'string' index type 'ParserRule'.
    readonly Array?: ArrayRule;
    readonly [type: string]: ParserRule;
};

type OutputRules<Rule> = {
    // @ts-expect-error - TS2411 - Property 'Array' of type 'ArrayRule | undefined' is not assignable to 'string' index type 'Rule'.
    readonly Array?: ArrayRule;
    readonly [type: string]: Rule;
};
type Rules<OutputRule> = {
    // @ts-expect-error - TS2411 - Property 'Array' of type 'ArrayRule | undefined' is not assignable to 'string' index type 'ParserRule & OutputRule'.
    readonly Array?: ArrayRule;
    readonly [type: string]: ParserRule & OutputRule;
};
type ReactRules = {
    // @ts-expect-error - TS2411 - Property 'Array' of type '{ readonly react: ArrayNodeOutput<ReactNode>; } | undefined' is not assignable to 'string' index type 'ParserRule & ReactOutputRule'.
    readonly Array?: {
        readonly react: ArrayNodeOutput<ReactElements>;
    };
    readonly [type: string]: ParserRule & ReactOutputRule;
};
type HtmlRules = {
    // @ts-expect-error - TS2411 - Property 'Array' of type '{ readonly html: ArrayNodeOutput<string>; } | undefined' is not assignable to 'string' index type 'ParserRule & HtmlOutputRule'.
    readonly Array?: {
        readonly html: ArrayNodeOutput<string>;
    };
    readonly [type: string]: ParserRule & HtmlOutputRule;
};

// We want to clarify our defaultRules types a little bit more so clients can
// reuse defaultRules built-ins. So we make some stronger guarantess when
// we can:
type NonNullReactOutputRule = {
    readonly react: ReactNodeOutput;
};
type ElementReactOutputRule = {
    readonly react: NodeOutput<ReactElement>;
};
type TextReactOutputRule = {
    readonly react: NodeOutput<string>;
};
type NonNullHtmlOutputRule = {
    readonly html: HtmlNodeOutput;
};

type DefaultInRule = SingleNodeParserRule & ReactOutputRule & HtmlOutputRule;
type TextInOutRule = SingleNodeParserRule &
    TextReactOutputRule &
    NonNullHtmlOutputRule;
type LenientInOutRule = SingleNodeParserRule &
    NonNullReactOutputRule &
    NonNullHtmlOutputRule;
type DefaultInOutRule = SingleNodeParserRule &
    ElementReactOutputRule &
    NonNullHtmlOutputRule;

type DefaultRules = {
    readonly Array: {
        readonly react: ArrayNodeOutput<ReactElements>;
        readonly html: ArrayNodeOutput<string>;
    };
    readonly heading: DefaultInOutRule;
    readonly nptable: DefaultInRule;
    readonly lheading: DefaultInRule;
    readonly hr: DefaultInOutRule;
    readonly codeBlock: DefaultInOutRule;
    readonly fence: DefaultInRule;
    readonly blockQuote: DefaultInOutRule;
    readonly list: DefaultInOutRule;
    readonly def: LenientInOutRule;
    readonly table: DefaultInOutRule;
    readonly tableSeparator: DefaultInRule;
    readonly newline: TextInOutRule;
    readonly paragraph: DefaultInOutRule;
    readonly escape: DefaultInRule;
    readonly autolink: DefaultInRule;
    readonly mailto: DefaultInRule;
    readonly url: DefaultInRule;
    readonly link: DefaultInOutRule;
    readonly image: DefaultInOutRule;
    readonly reflink: DefaultInRule;
    readonly refimage: DefaultInRule;
    readonly em: DefaultInOutRule;
    readonly strong: DefaultInOutRule;
    readonly u: DefaultInOutRule;
    readonly del: DefaultInOutRule;
    readonly inlineCode: DefaultInOutRule;
    readonly br: DefaultInOutRule;
    readonly text: TextInOutRule;
};

type RefNode = {
    type: string;
    content?: ASTNode;
    target?: string;
    title?: string;
    alt?: string;
};

// End TypeScript Definitions

const CR_NEWLINE_R = /\r\n?/g;
const TAB_R = /\t/g;
const FORMFEED_R = /\f/g;

/**
 * Turn constious whitespace into easy-to-process whitespace
 */
const preprocess = function (source: string): string {
    return source
        .replace(CR_NEWLINE_R, "\n")
        .replace(FORMFEED_R, "")
        .replace(TAB_R, "    ");
};

const populateInitialState = function (
    givenState?: State | null,
    defaultState?: State | null,
): State {
    const state: State = givenState || {};
    if (defaultState != null) {
        for (const prop in defaultState) {
            if (Object.prototype.hasOwnProperty.call(defaultState, prop)) {
                state[prop] = defaultState[prop];
            }
        }
    }
    return state;
};

/**
 * Creates a parser for a given set of rules, with the precedence
 * specified as a list of rules.
 *
 * @param {SimpleMarkdown.ParserRules} rules
 *     an object containing
 *     rule type -> {match, order, parse} objects
 *     (lower order is higher precedence)
 * @param {SimpleMarkdown.OptionalState} [defaultState]
 *
 * @returns {SimpleMarkdown.Parser}
 *     The resulting parse function, with the following parameters:
 *     @source: the input source string to be parsed
 *     @state: an optional object to be threaded through parse
 *         calls. Allows clients to add stateful operations to
 *         parsing, such as keeping track of how many levels deep
 *         some nesting is. For an example use-case, see passage-ref
 *         parsing in src/widgets/passage/passage-markdown.jsx
 */
const parserFor = function (
    rules: ParserRules,
    defaultState?: State | null,
): Parser {
    // Sorts rules in order of increasing order, then
    // ascending rule name in case of ties.
    const ruleList = Object.keys(rules).filter(function (type) {
        const rule = rules[type];
        if (rule == null || rule.match == null) {
            return false;
        }
        const order = rule.order;
        if (
            (typeof order !== "number" || !isFinite(order)) &&
            typeof console !== "undefined"
        ) {
            console.warn(
                "simple-markdown: Invalid order for rule `" +
                type +
                "`: " +
                String(order),
            );
        }
        return true;
    });

    ruleList.sort(function (typeA, typeB) {
        const ruleA: ParserRule = rules[typeA] as any;
        const ruleB: ParserRule = rules[typeB] as any;
        const orderA = ruleA.order;
        const orderB = ruleB.order;

        // First sort based on increasing order
        if (orderA !== orderB) {
            return orderA - orderB;
        }

        const secondaryOrderA = ruleA.quality ? 0 : 1;
        const secondaryOrderB = ruleB.quality ? 0 : 1;

        if (secondaryOrderA !== secondaryOrderB) {
            return secondaryOrderA - secondaryOrderB;

            // Then based on increasing unicode lexicographic ordering
        } else if (typeA < typeB) {
            return -1;
        } else if (typeA > typeB) {
            return 1;
        } else {
            // Rules should never have the same name,
            // but this is provided for completeness.
            return 0;
        }
    });

    let latestState: State;
    const nestedParse: Parser = function (
        source: string,
        state?: State | null,
    ): Array<SingleASTNode | UnTypedASTNode> {
        const result: Array<SingleASTNode | UnTypedASTNode> = [];
        state = state || latestState;
        latestState = state;
        while (source) {
            // store the best match, it's rule, and quality:
            let ruleType = null;
            let rule = null;
            let capture = null;
            let quality = NaN;

            // loop control constiables:
            let i = 0;
            let currRuleType = ruleList[0];

            let currRule: ParserRule = rules[currRuleType];

            do {
                // const currOrder = currRule.order;
                const prevCaptureStr =
                    state.prevCapture == null ? "" : state.prevCapture[0];
                const currCapture = currRule.match(source, state, prevCaptureStr);

                if (currCapture) {
                    const currQuality = currRule.quality
                        ? currRule.quality(currCapture, state, prevCaptureStr)
                        : 0;
                    // This should always be true the first time because
                    // the initial quality is NaN (that's why there's the
                    // condition negation).
                    if (!(currQuality <= quality)) {
                        //// @ts-expect-error - TS2322 - Type 'string' is not assignable to type 'null'.
                        ruleType = currRuleType;
                        //// @ts-expect-error - TS2322 - Type 'ParserRule' is not assignable to type 'null'.
                        rule = currRule;
                        //// @ts-expect-error - TS2322 - Type 'Capture' is not assignable to type 'null'.
                        capture = currCapture;
                        quality = currQuality;
                    }
                }

                // Move on to the next item.
                // Note that this makes `currRule` be the next item
                i++;
                currRuleType = ruleList[i];
                currRule = rules[currRuleType];
            } while (
                // keep looping while we're still within the ruleList
                currRule &&
                // if we don't have a match yet, continue
                (!capture ||
                    // or if we have a match, but the next rule is
                    // at the same order, and has a quality measurement
                    // functions, then this rule must have a quality
                    // measurement function (since they are sorted before
                    // those without), and we need to check if there is
                    // a better quality match
                    (currRule.order === currOrder && currRule.quality))
            );

            // TODO(aria): Write tests for these
            if (rule == null || capture == null) {
                throw new Error(
                    "Could not find a matching rule for the below " +
                    "content. The rule with highest `order` should " +
                    "always match content provided to it. Check " +
                    "the definition of `match` for '" +
                    ruleList[ruleList.length - 1] +
                    "'. It seems to not match the following source:\n" +
                    source,
                );
            }
            //// @ts-expect-error - TS2339 - Property 'index' does not exist on type 'never'.
            if (capture.index) {
                // If present and non-zero, i.e. a non-^ regexp result:
                throw new Error(
                    "`match` must return a capture starting at index 0 " +
                    "(the current parse index). Did you forget a ^ at the " +
                    "start of the RegExp?",
                );
            }

            //// @ts-expect-error - TS2339 - Property 'parse' does not exist on type 'never'.
            const parsed = rule.parse(capture, nestedParse, state);
            // We maintain the same object here so that rules can
            // store references to the objects they return and
            // modify them later. (oops sorry! but this adds a lot
            // of power--see reflinks.)
            if (Array.isArray(parsed)) {
                Array.prototype.push.apply(result, parsed);
            } else {
                if (parsed == null || typeof parsed !== "object") {
                    throw new Error(
                        `parse() function returned invalid parse result: '${parsed}'`,
                    );
                }

                // We also let rules override the default type of
                // their parsed node if they would like to, so that
                // there can be a single output function for all links,
                // even if there are several rules to parse them.
                if (parsed.type == null) {
                    parsed.type = ruleType;
                }
                result.push(parsed);
            }

            state.prevCapture = capture;
            source = source.substring(state.prevCapture[0].length);
        }

        return result;
    };

    const outerParse: Parser = function (
        source: string,
        state?: State | null,
    ): Array<SingleASTNode | UnTypedASTNode> {
        latestState = populateInitialState(state, defaultState);
        if (!latestState.inline && !latestState.disableAutoBlockNewlines) {
            source = source + "\n\n";
        }
        // We store the previous capture so that match functions can
        // use some limited amount of lookbehind. Lists use this to
        // ensure they don't match arbitrary '- ' or '* ' in inline
        // text (see the list rule for more information). This stores
        // the full regex capture object, if there is one.
        latestState.prevCapture = null;
        return nestedParse(preprocess(source), latestState);
    };

    return outerParse;
};

// Creates a match function for an inline scoped element from a regex
const inlineRegex = function (regex: RegExp): MatchFunction {
    const match = function (
        source: string,
        state: State,
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        _prevCapture: string,
    ): Capture | null | undefined {
        if (state.inline) {
            return regex.exec(source);
        } else {
            return null;
        }
    };
    match.regex = regex;

    return match;
};

// Creates a match function for a block scoped element from a regex
const blockRegex = function (regex: RegExp): MatchFunction {
    const match: MatchFunction = function (source, state) {
        if (state.inline) {
            return null;
        } else {
            return regex.exec(source);
        }
    };
    match.regex = regex;
    return match;
};

// Creates a match function from a regex, ignoring block/inline scope
const anyScopeRegex = function (regex: RegExp): MatchFunction {
    const match: MatchFunction = function (source, _state) {
        return regex.exec(source);
    };
    match.regex = regex;
    return match;
};

const TYPE_SYMBOL =
    (typeof Symbol === "function" &&
        Symbol.for &&
        Symbol.for("react.element")) ||
    0xeac7;

const reactElement = function (
    type: string,
    key: string | number | null | undefined,
    props: {
        [key: string]: any;
    },
): ReactElement {
    const element: ReactElement = {
        $$typeof: TYPE_SYMBOL,
        type: type,
        key: key == null ? undefined : key,
        ref: null,
        props: props,
        _owner: null,
    } as any;
    return element;
};

/** Returns a closed HTML tag.
 * @param {string} tagName - Name of HTML tag (eg. "em" or "a")
 * @param {string} content - Inner content of tag
 * @param {{ [attr: string]: SimpleMarkdown.Attr }} [attributes] - Optional extra attributes of tag as an object of key-value pairs
 *   eg. { "href": "http://google.com" }. Falsey attributes are filtered out.
 * @param {boolean} [isClosed] - boolean that controls whether tag is closed or not (eg. img tags).
 *   defaults to true
 */
const htmlTag = function (
    tagName: string,
    content: string,
    attributes?: Partial<Record<any, Attr | null | undefined>> | null,
    isClosed?: boolean | null,
) {
    attributes = attributes || {};
    isClosed = typeof isClosed !== "undefined" ? isClosed : true;

    let attributeString = "";
    for (const attr in attributes) {
        const attribute = attributes[attr];
        // Removes falsey attributes
        if (
            Object.prototype.hasOwnProperty.call(attributes, attr) &&
            attribute
        ) {
            attributeString +=
                " " + sanitizeText(attr) + '="' + sanitizeText(attribute) + '"';
        }
    }

    const unclosedTag = "<" + tagName + attributeString + ">";

    if (isClosed) {
        return unclosedTag + content + "</" + tagName + ">";
    } else {
        return unclosedTag;
    }
};

const EMPTY_PROPS: Record<string, any> = {};

/**
 * @param {string | null | undefined} url - url to sanitize
 * @returns {string | null} - url if safe, or null if a safe url could not be made
 */
const sanitizeUrl = function (url?: string | null) {
    if (url == null) {
        return null;
    }
    try {
        const prot = new URL(url, "https://localhost").protocol;
        if (
            prot.indexOf("javascript:") === 0 ||
            prot.indexOf("vbscript:") === 0 ||
            prot.indexOf("data:") === 0
        ) {
            return null;
        }
    } catch (e: any) {
        // invalid URLs should throw a TypeError
        // see for instance: `new URL("");`
        return null;
    }
    return url;
};

const SANITIZE_TEXT_R = /[<>&"']/g;
const SANITIZE_TEXT_CODES = {
    "<": "&lt;",
    ">": "&gt;",
    "&": "&amp;",
    '"': "&quot;",
    "'": "&#x27;",
    "/": "&#x2F;",
    "`": "&#96;",
};

const sanitizeText = function (text: Attr): string {
    return String(text).replace(SANITIZE_TEXT_R, function (chr) {
        return SANITIZE_TEXT_CODES[chr as keyof typeof SANITIZE_TEXT_CODES];
    });
};

const UNESCAPE_URL_R = /\\([^0-9A-Za-z\s])/g;

const unescapeUrl = function (rawUrlString: string): string {
    return rawUrlString.replace(UNESCAPE_URL_R, "$1");
};

/**
 * Parse some content with the parser `parse`, with state.inline
 * set to true. Useful for block elements; not generally necessary
 * to be used by inline elements (where state.inline is already true.
 */
const parseInline = function (
    parse: Parser,
    content: string,
    state: State,
): ASTNode {
    const isCurrentlyInline = state.inline || false;
    state.inline = true;
    const result = parse(content, state);
    state.inline = isCurrentlyInline;
    return result;
};

const parseBlock = function (
    parse: Parser,
    content: string,
    state: State,
): ASTNode {
    const isCurrentlyInline = state.inline || false;
    state.inline = false;
    const result = parse(content + "\n\n", state);
    state.inline = isCurrentlyInline;
    return result;
};

const parseCaptureInline = function (
    capture: Capture,
    parse: Parser,
    state: State,
): UnTypedASTNode {
    return {
        content: parseInline(parse, capture[1], state),
    };
};

const ignoreCapture = function (): UnTypedASTNode {
    return {};
};

// recognize a `*` `-`, `+`, `1.`, `2.`... list bullet
const LIST_BULLET = "(?:[*+-]|\\d+\\.)";
// recognize the start of a list item:
// leading space plus a bullet plus a space (`   * `)
const LIST_ITEM_PREFIX = "( *)(" + LIST_BULLET + ") +";
const LIST_ITEM_PREFIX_R = new RegExp("^" + LIST_ITEM_PREFIX);
// recognize an individual list item:
//  * hi
//    this is part of the same item
//
//    as is this, which is a new paragraph in the same item
//
//  * but this is not part of the same item
const LIST_ITEM_R = new RegExp(
    LIST_ITEM_PREFIX +
    "[^\\n]*(?:\\n" +
    "(?!\\1" +
    LIST_BULLET +
    " )[^\\n]*)*(\n|$)",
    "gm",
);
const BLOCK_END_R = /\n{2,}$/;
const INLINE_CODE_ESCAPE_BACKTICKS_R = /^ (?= *`)|(` *) $/g;
// recognize the end of a paragraph block inside a list item:
// two or more newlines at end end of the item
const LIST_BLOCK_END_R = BLOCK_END_R;
const LIST_ITEM_END_R = / *\n+$/;
// check whether a list item has paragraphs: if it does,
// we leave the newlines at the end
const LIST_R = new RegExp(
    "^( *)(" +
    LIST_BULLET +
    ") " +
    "[\\s\\S]+?(?:\n{2,}(?! )" +
    "(?!\\1" +
    LIST_BULLET +
    " )\\n*" +
    // the \\s*$ here is so that we can parse the inside of nested
    // lists, where our content might end before we receive two `\n`s
    "|\\s*\n*$)",
);
const LIST_LOOKBEHIND_R = /(?:^|\n)( *)$/;

const TABLES = (function () {
    // predefine regexes so we don't have to create them inside functions
    // sure, regex literals should be fast, even inside functions, but they
    // aren't in all browsers.
    // const TABLE_BLOCK_TRIM = /\n+/g;
    const TABLE_ROW_SEPARATOR_TRIM = /^ *\| *| *\| *$/g;
    const TABLE_CELL_END_TRIM = / *$/;
    const TABLE_RIGHT_ALIGN = /^ *-+: *$/;
    const TABLE_CENTER_ALIGN = /^ *:-+: *$/;
    const TABLE_LEFT_ALIGN = /^ *:-+ *$/;

    // TODO: This needs a real type
    type TableAlignment = any;

    const parseTableAlignCapture = function (
        alignCapture: string,
    ): TableAlignment {
        if (TABLE_RIGHT_ALIGN.test(alignCapture)) {
            return "right";
        } else if (TABLE_CENTER_ALIGN.test(alignCapture)) {
            return "center";
        } else if (TABLE_LEFT_ALIGN.test(alignCapture)) {
            return "left";
        } else {
            return null;
        }
    };

    const parseTableAlign = function (
        source: string,
        _parse: Parser,
        _state: State,
        trimEndSeparators: boolean,
    ): Array<TableAlignment> {
        if (trimEndSeparators) {
            source = source.replace(TABLE_ROW_SEPARATOR_TRIM, "");
        }
        const alignText = source.trim().split("|");
        return alignText.map(parseTableAlignCapture);
    };

    const parseTableRow = function (
        source: string,
        parse: Parser,
        state: State,
        trimEndSeparators: boolean,
    ): Array<Array<SingleASTNode | UnTypedASTNode>> {
        const prevInTable = state.inTable;
        state.inTable = true;
        const tableRow = parse(source.trim(), state);
        state.inTable = prevInTable;

        const cells = [[]];
        tableRow.forEach(function (node, i) {
            if (node.type === "tableSeparator") {
                // Filter out empty table separators at the start/end:
                if (
                    !trimEndSeparators ||
                    (i !== 0 && i !== tableRow.length - 1)
                ) {
                    // Split the current row:
                    cells.push([]);
                }
            } else {
                if (
                    node.type === "text" &&
                    (tableRow[i + 1] == null ||
                        tableRow[i + 1].type === "tableSeparator")
                ) {
                    node.content = node.content.replace(
                        TABLE_CELL_END_TRIM,
                        "",
                    );
                }
                // @ts-expect-error - TS2345 - Argument of type 'SingleASTNode' is not assignable to parameter of type 'never'.
                cells[cells.length - 1].push(node);
            }
        });

        return cells;
    };

    /**
     * @param {string} source
     * @param {SimpleMarkdown.Parser} parse
     * @param {SimpleMarkdown.State} state
     * @param {boolean} trimEndSeparators
     * @returns {SimpleMarkdown.ASTNode[][]}
     */
    const parseTableCells = function (
        source: string,
        parse: Parser,
        state: State,
        trimEndSeparators: boolean,
    ): Array<Array<ASTNode>> {
        const rowsText = source.trim().split("\n");

        return rowsText.map(function (rowText) {
            return parseTableRow(rowText, parse, state, trimEndSeparators);
        });
    };

    /**
     * @param {boolean} trimEndSeparators
     * @returns {SimpleMarkdown.SingleNodeParseFunction}
     */
    const parseTable = function (trimEndSeparators: boolean) {
        return function (capture: Capture, parse: Parser, state: State) {
            state.inline = true;
            const header = parseTableRow(
                capture[1],
                parse,
                state,
                trimEndSeparators,
            );
            const align = parseTableAlign(
                capture[2],
                parse,
                state,
                trimEndSeparators,
            );
            const cells = parseTableCells(
                capture[3],
                parse,
                state,
                trimEndSeparators,
            );
            state.inline = false;

            return {
                type: "table",
                header: header,
                align: align,
                cells: cells,
            };
        };
    };

    return {
        parseTable: parseTable(true),
        parseNpTable: parseTable(false),
        TABLE_REGEX:
            /^ *(\|.+)\n *\|( *[-:]+[-| :]*)\n((?: *\|.*(?:\n|$))*)\n*/,
        NPTABLE_REGEX:
            /^ *(\S.*\|.*)\n *([-:]+ *\|[-| :]*)\n((?:.*\|.*(?:\n|$))*)\n*/,
    };
})();

const LINK_INSIDE = "(?:\\[[^\\]]*\\]|[^\\[\\]]|\\](?=[^\\[]*\\]))*";
const LINK_HREF_AND_TITLE =
    "\\s*<?((?:\\([^)]*\\)|[^\\s\\\\]|\\\\.)*?)>?(?:\\s+['\"]([\\s\\S]*?)['\"])?\\s*";
const AUTOLINK_MAILTO_CHECK_R = /mailto:/i;

const parseRef = function (
    capture: Capture,
    state: State,
    refNode: RefNode,
): RefNode {
    const ref = (capture[2] || capture[1]).replace(/\s+/g, " ").toLowerCase();

    // We store information about previously seen defs on
    // state._defs (_ to deconflict with client-defined
    // state). If the def for this reflink/refimage has
    // already been seen, we can use its target/source
    // and title here:
    if (state._defs && state._defs[ref]) {
        const def = state._defs[ref];
        // `refNode` can be a link or an image. Both use
        // target and title properties.
        refNode.target = def.target;
        refNode.title = def.title;
    }

    // In case we haven't seen our def yet (or if someone
    // overwrites that def later on), we add this node
    // to the list of ref nodes for that def. Then, when
    // we find the def, we can modify this link/image AST
    // node :).
    // I'm sorry.
    state._refs = state._refs || {};
    state._refs[ref] = state._refs[ref] || [];
    state._refs[ref].push(refNode);

    return refNode;
};

let currOrder = 0;

const defaultRules: DefaultRules = {
    Array: {
        react: function (arr, output, state) {
            const oldKey = state.key;
            const result: Array<ReactElements> = [];

            // map output over the ast, except group any text
            // nodes together into a single string output.
            for (let i = 0, key = 0; i < arr.length; i++, key++) {
                // `key` is our numerical `state.key`, which we increment for
                // every output node, but don't change for joined text nodes.
                // (i, however, must change for joined text nodes)
                state.key = "" + i;

                let node = arr[i];
                if (node.type === "text") {
                    node = { type: "text", content: node.content };
                    for (
                        ;
                        i + 1 < arr.length && arr[i + 1].type === "text";
                        i++
                    ) {
                        node.content += arr[i + 1].content;
                    }
                }

                result.push(output(node, state));
            }

            state.key = oldKey;
            return result;
        },
        html: function (arr, output, state) {
            let result = "";

            // map output over the ast, except group any text
            // nodes together into a single string output.
            for (let i = 0; i < arr.length; i++) {
                let node = arr[i];
                if (node.type === "text") {
                    node = { type: "text", content: node.content };
                    for (
                        ;
                        i + 1 < arr.length && arr[i + 1].type === "text";
                        i++
                    ) {
                        node.content += arr[i + 1].content;
                    }
                }

                result += output(node, state);
            }
            return result;
        },
    },
    heading: {
        order: currOrder++,
        match: blockRegex(/^ *(#{1,6})([^\n]+?)#* *(?:\n *)+\n/),
        parse: function (capture, parse, state) {
            return {
                level: capture[1].length,
                content: parseInline(parse, capture[2].trim(), state),
            };
        },
        react: function (node, output, state) {
            return reactElement("h" + node.level, state.key, {
                children: output(node.content, state),
            });
        },
        html: function (node, output, state) {
            return htmlTag("h" + node.level, output(node.content, state));
        },
    },
    nptable: {
        order: currOrder++,
        match: blockRegex(TABLES.NPTABLE_REGEX),
        parse: TABLES.parseNpTable,
        react: null,
        html: null,
    },
    lheading: {
        order: currOrder++,
        match: blockRegex(/^([^\n]+)\n *(=|-){3,} *(?:\n *)+\n/),
        parse: function (capture, parse, state) {
            return {
                type: "heading",
                level: capture[2] === "=" ? 1 : 2,
                content: parseInline(parse, capture[1], state),
            };
        },
        react: null,
        html: null,
    },
    hr: {
        order: currOrder++,
        match: blockRegex(/^( *[-*_]){3,} *(?:\n *)+\n/),
        parse: ignoreCapture,
        react: function (_node, _output, state) {
            return reactElement("hr", state.key, EMPTY_PROPS);
        },
        html: function (_node, _output, _state) {
            return "<hr>";
        },
    },
    codeBlock: {
        order: currOrder++,
        match: blockRegex(/^(?: {4}[^\n]+\n*)+(?:\n *)+\n/),
        parse: function (capture, _parse, _state) {
            const content = capture[0].replace(/^ {4}/gm, "").replace(/\n+$/, "");
            return {
                lang: undefined,
                content: content,
            };
        },
        react: function (node, _output, state) {
            const className = node.lang
                ? "markdown-code-" + node.lang
                : undefined;

            return reactElement("pre", state.key, {
                children: reactElement("code", null, {
                    className: className,
                    children: node.content,
                }),
            });
        },
        html: function (node, _output, _state) {
            const className = node.lang
                ? "markdown-code-" + node.lang
                : undefined;

            const codeBlock = htmlTag("code", sanitizeText(node.content), {
                class: className,
            });
            return htmlTag("pre", codeBlock);
        },
    },
    fence: {
        order: currOrder++,
        match: blockRegex(
            /^ *(`{3,}|~{3,}) *(?:(\S+) *)?\n([\s\S]+?)\n?\1 *(?:\n *)+\n/,
        ),
        parse: function (capture, _parse, _state) {
            return {
                type: "codeBlock",
                lang: capture[2] || undefined,
                content: capture[3],
            };
        },
        react: null,
        html: null,
    },
    blockQuote: {
        order: currOrder++,
        match: blockRegex(/^( *>[^\n]+(\n[^\n]+)*\n*)+\n{2,}/),
        parse: function (capture, parse, state) {
            const content = capture[0].replace(/^ *> ?/gm, "");
            return {
                content: parse(content, state),
            };
        },
        react: function (node, output, state) {
            return reactElement("blockquote", state.key, {
                children: output(node.content, state),
            });
        },
        html: function (node, output, state) {
            return htmlTag("blockquote", output(node.content, state));
        },
    },
    list: {
        order: currOrder++,
        match: function (source, state) {
            // We only want to break into a list if we are at the start of a
            // line. This is to avoid parsing "hi * there" with "* there"
            // becoming a part of a list.
            // You might wonder, "but that's inline, so of course it wouldn't
            // start a list?". You would be correct! Except that some of our
            // lists can be inline, because they might be inside another list,
            // in which case we can parse with inline scope, but need to allow
            // nested lists inside this inline scope.
            const prevCaptureStr =
                state.prevCapture == null ? "" : state.prevCapture[0];
            const isStartOfLineCapture = LIST_LOOKBEHIND_R.exec(prevCaptureStr);
            const isListBlock = state._list || !state.inline;

            if (isStartOfLineCapture && isListBlock) {
                source = isStartOfLineCapture[1] + source;
                return LIST_R.exec(source);
            } else {
                return null;
            }
        },
        parse: function (capture, parse, state) {
            const bullet = capture[2];
            const ordered = bullet.length > 1;
            const start = ordered ? +bullet : undefined;
            // @ts-expect-error - TS2322 - Type 'RegExpMatchArray | null' is not assignable to type 'string[]'.
            const items: Array<string> = capture[0]
                .replace(LIST_BLOCK_END_R, "\n")
                .match(LIST_ITEM_R);

            // We know this will match here, because of how the regexes are
            // defined

            let lastItemWasAParagraph = false;
            const itemContent = items.map(function (item: string, i: number) {
                // We need to see how far indented this item is:
                const prefixCapture = LIST_ITEM_PREFIX_R.exec(item);
                const space = prefixCapture ? prefixCapture[0].length : 0;
                // And then we construct a regex to "unindent" the subsequent
                // lines of the items by that amount:
                const spaceRegex = new RegExp("^ {1," + space + "}", "gm");

                // Before processing the item, we need a couple things
                const content = item
                    // remove indents on trailing lines:
                    .replace(spaceRegex, "")
                    // remove the bullet:
                    .replace(LIST_ITEM_PREFIX_R, "");

                // I'm not sur4 why this is necessary again?

                // Handling "loose" lists, like:
                //
                //  * this is wrapped in a paragraph
                //
                //  * as is this
                //
                //  * as is this
                const isLastItem = i === items.length - 1;
                const containsBlocks = content.indexOf("\n\n") !== -1;

                // Any element in a list is a block if it contains multiple
                // newlines. The last element in the list can also be a block
                // if the previous item in the list was a block (this is
                // because non-last items in the list can end with \n\n, but
                // the last item can't, so we just "inherit" this property
                // from our previous element).
                const thisItemIsAParagraph =
                    containsBlocks || (isLastItem && lastItemWasAParagraph);
                lastItemWasAParagraph = thisItemIsAParagraph;

                // backup our state for restoration afterwards. We're going to
                // want to set state._list to true, and state.inline depending
                // on our list's looseness.
                const oldStateInline = state.inline;
                const oldStateList = state._list;
                state._list = true;

                // Parse inline if we're in a tight list, or block if we're in
                // a loose list.
                let adjustedContent;
                if (thisItemIsAParagraph) {
                    state.inline = false;
                    adjustedContent = content.replace(LIST_ITEM_END_R, "\n\n");
                } else {
                    state.inline = true;
                    adjustedContent = content.replace(LIST_ITEM_END_R, "");
                }

                const result = parse(adjustedContent, state);

                // Restore our state before returning
                state.inline = oldStateInline;
                state._list = oldStateList;
                return result;
            });

            return {
                ordered: ordered,
                start: start,
                items: itemContent,
            };
        },
        react: function (node, output, state) {
            const ListWrapper = node.ordered ? "ol" : "ul";

            return reactElement(ListWrapper, state.key, {
                start: node.start,
                children: node.items.map(function (item: ASTNode, i: number) {
                    return reactElement("li", "" + i, {
                        children: output(item, state),
                    });
                }),
            });
        },
        html: function (node, output, state) {
            const listItems = node.items
                .map(function (item: ASTNode) {
                    return htmlTag("li", output(item, state));
                })
                .join("");

            const listTag = node.ordered ? "ol" : "ul";
            const attributes = {
                start: node.start,
            };
            return htmlTag(listTag, listItems, attributes);
        },
    },
    def: {
        order: currOrder++,
        // TODO(aria): This will match without a blank line before the next
        // block element, which is inconsistent with most of the rest of
        // simple-markdown.
        match: blockRegex(
            /^ *\[([^\]]+)\]: *<?([^\s>]*)>?(?: +["(]([^\n]+)[")])? *\n(?: *\n)*/,
        ),
        parse: function (capture, _parse, state) {
            const def = capture[1].replace(/\s+/g, " ").toLowerCase();
            const target = capture[2];
            const title = capture[3];

            // Look for previous links/images using this def
            // If any links/images using this def have already been declared,
            // they will have added themselves to the state._refs[def] list
            // (_ to deconflict with client-defined state). We look through
            // that list of reflinks for this def, and modify those AST nodes
            // with our newly found information now.
            // Sorry :(.
            if (state._refs && state._refs[def]) {
                // `refNode` can be a link or an image
                state._refs[def].forEach(function (refNode: RefNode) {
                    refNode.target = target;
                    refNode.title = title;
                });
            }

            // Add this def to our map of defs for any future links/images
            // In case we haven't found any or all of the refs referring to
            // this def yet, we add our def to the table of known defs, so
            // that future reflinks can modify themselves appropriately with
            // this information.
            state._defs = state._defs || {};
            state._defs[def] = {
                target: target,
                title: title,
            };

            // return the relevant parsed information
            // for debugging only.
            return {
                def: def,
                target: target,
                title: title,
            };
        },
        react: function () {
            return null;
        },
        html: function () {
            return "";
        },
    },
    table: {
        order: currOrder++,
        match: blockRegex(TABLES.TABLE_REGEX),
        parse: TABLES.parseTable,
        react: function (node, output, state) {
            const getStyle = function (colIndex: number): {
                [attr: string]: Attr;
            } {
                return node.align[colIndex] == null
                    ? {}
                    : {
                        textAlign: node.align[colIndex],
                    };
            };

            const headers = node.header.map(function (
                content: ASTNode,
                i: number,
            ) {
                return reactElement("th", "" + i, {
                    style: getStyle(i),
                    scope: "col",
                    children: output(content, state),
                });
            });

            const rows = node.cells.map(function (
                row: Array<ASTNode>,
                r: number,
            ) {
                return reactElement("tr", "" + r, {
                    children: row.map(function (content: ASTNode, c: number) {
                        return reactElement("td", "" + c, {
                            style: getStyle(c),
                            children: output(content, state),
                        });
                    }),
                });
            });

            return reactElement("table", state.key, {
                children: [
                    reactElement("thead", "thead", {
                        children: reactElement("tr", null, {
                            children: headers,
                        }),
                    }),
                    reactElement("tbody", "tbody", {
                        children: rows,
                    }),
                ],
            });
        },
        html: function (node, output, state) {
            const getStyle = function (colIndex: number): string {
                return node.align[colIndex] == null
                    ? ""
                    : "text-align:" + node.align[colIndex] + ";";
            };

            const headers = node.header
                .map(function (content: ASTNode, i: number) {
                    return htmlTag("th", output(content, state), {
                        style: getStyle(i),
                        scope: "col",
                    });
                })
                .join("");

            const rows = node.cells
                .map(function (row: Array<ASTNode>) {
                    const cols = row
                        .map(function (content: ASTNode, c: number) {
                            return htmlTag("td", output(content, state), {
                                style: getStyle(c),
                            });
                        })
                        .join("");

                    return htmlTag("tr", cols);
                })
                .join("");

            const thead = htmlTag("thead", htmlTag("tr", headers));
            const tbody = htmlTag("tbody", rows);

            return htmlTag("table", thead + tbody);
        },
    },
    newline: {
        order: currOrder++,
        match: blockRegex(/^(?:\n *)*\n/),
        parse: ignoreCapture,
        react: function (_node, _output, _state) {
            return "\n";
        },
        html: function (_node, _output, _state) {
            return "\n";
        },
    },
    paragraph: {
        order: currOrder++,
        match: blockRegex(/^((?:[^\n]|\n(?! *\n))+)(?:\n *)+\n/),
        parse: parseCaptureInline,
        react: function (node, output, state) {
            return reactElement("div", state.key, {
                className: "paragraph",
                children: output(node.content, state),
            });
        },
        html: function (node, output, state) {
            const attributes = {
                class: "paragraph",
            };
            return htmlTag("div", output(node.content, state), attributes);
        },
    },
    escape: {
        order: currOrder++,
        // We don't allow escaping numbers, letters, or spaces here so that
        // backslashes used in plain text still get rendered. But allowing
        // escaping anything else provides a very flexible escape mechanism,
        // regardless of how this grammar is extended.
        match: inlineRegex(/^\\([^0-9A-Za-z\s])/),
        parse: function (capture, _parse, _state) {
            return {
                type: "text",
                content: capture[1],
            };
        },
        react: null,
        html: null,
    },
    tableSeparator: {
        order: currOrder++,
        match: function (source, state) {
            if (!state.inTable) {
                return null;
            }
            return /^ *\| */.exec(source);
        },
        parse: function () {
            return { type: "tableSeparator" };
        },
        // These shouldn't be reached, but in case they are, be reasonable:
        react: function () {
            return " | ";
        },
        html: function () {
            return " &vert; ";
        },
    },
    autolink: {
        order: currOrder++,
        match: inlineRegex(/^<([^: >]+:\/[^ >]+)>/),
        parse: function (capture, _parse, _state) {
            return {
                type: "link",
                content: [
                    {
                        type: "text",
                        content: capture[1],
                    },
                ],
                target: capture[1],
            };
        },
        react: null,
        html: null,
    },
    mailto: {
        order: currOrder++,
        match: inlineRegex(/^<([^ >]+@[^ >]+)>/),
        parse: function (capture, _parse, _state) {
            const address = capture[1];
            let target = capture[1];

            // Check for a `mailto:` already existing in the link:
            if (!AUTOLINK_MAILTO_CHECK_R.test(target)) {
                target = "mailto:" + target;
            }

            return {
                type: "link",
                content: [
                    {
                        type: "text",
                        content: address,
                    },
                ],
                target: target,
            };
        },
        react: null,
        html: null,
    },
    url: {
        order: currOrder++,
        match: inlineRegex(/^(https?:\/\/[^\s<]+[^<.,:;"')\]\s])/),
        parse: function (capture, _parse, _state) {
            return {
                type: "link",
                content: [
                    {
                        type: "text",
                        content: capture[1],
                    },
                ],
                target: capture[1],
                title: undefined,
            };
        },
        react: null,
        html: null,
    },
    link: {
        order: currOrder++,
        match: inlineRegex(
            new RegExp(
                "^\\[(" + LINK_INSIDE + ")\\]\\(" + LINK_HREF_AND_TITLE + "\\)",
            ),
        ),
        parse: function (capture, parse, state) {
            const link = {
                content: parse(capture[1], state),
                target: unescapeUrl(capture[2]),
                title: capture[3],
            };
            return link;
        },
        react: function (node, output, state) {
            return reactElement("a", state.key, {
                href: sanitizeUrl(node.target),
                title: node.title,
                children: output(node.content, state),
            });
        },
        html: function (node, output, state) {
            const attributes = {
                href: sanitizeUrl(node.target),
                title: node.title,
            };

            return htmlTag("a", output(node.content, state), attributes);
        },
    },
    image: {
        order: currOrder++,
        match: inlineRegex(
            new RegExp(
                "^!\\[(" +
                LINK_INSIDE +
                ")\\]\\(" +
                LINK_HREF_AND_TITLE +
                "\\)",
            ),
        ),
        parse: function (capture, _parse, _state) {
            const image = {
                alt: capture[1],
                target: unescapeUrl(capture[2]),
                title: capture[3],
            };
            return image;
        },
        react: function (node, _output, state) {
            return reactElement("img", state.key, {
                src: sanitizeUrl(node.target),
                alt: node.alt,
                title: node.title,
            });
        },
        html: function (node, _output, _state) {
            const attributes = {
                src: sanitizeUrl(node.target),
                alt: node.alt,
                title: node.title,
            };

            return htmlTag("img", "", attributes, false);
        },
    },
    reflink: {
        order: currOrder++,
        match: inlineRegex(
            new RegExp(
                // The first [part] of the link
                "^\\[(" +
                LINK_INSIDE +
                ")\\]" +
                // The [ref] target of the link
                "\\s*\\[([^\\]]*)\\]",
            ),
        ),
        parse: function (capture, parse, state) {
            return parseRef(capture, state, {
                type: "link",
                content: parse(capture[1], state),
            });
        },
        react: null,
        html: null,
    },
    refimage: {
        order: currOrder++,
        match: inlineRegex(
            new RegExp(
                // The first [part] of the link
                "^!\\[(" +
                LINK_INSIDE +
                ")\\]" +
                // The [ref] target of the link
                "\\s*\\[([^\\]]*)\\]",
            ),
        ),
        parse: function (capture, _parse, state) {
            return parseRef(capture, state, {
                type: "image",
                alt: capture[1],
            });
        },
        react: null,
        html: null,
    },
    em: {
        order: currOrder /* same as strong/u */,
        match: inlineRegex(
            new RegExp(
                // only match _s surrounding words.
                "^\\b_" +
                "((?:__|\\\\[\\s\\S]|[^\\\\_])+?)_" +
                "\\b" +
                // Or match *s:
                "|" +
                // Only match *s that are followed by a non-space:
                "^\\*(?=\\S)(" +
                // Match at least one of:
                "(?:" +
                //  - `**`: so that bolds inside italics don't close the
                //          italics
                "\\*\\*|" +
                //  - escape sequence: so escaped *s don't close us
                "\\\\[\\s\\S]|" +
                //  - whitespace: followed by a non-* (we don't
                //          want ' *' to close an italics--it might
                //          start a list)
                "\\s+(?:\\\\[\\s\\S]|[^\\s\\*\\\\]|\\*\\*)|" +
                //  - non-whitespace, non-*, non-backslash characters
                "[^\\s\\*\\\\]" +
                ")+?" +
                // followed by a non-space, non-* then *
                ")\\*(?!\\*)",
            ),
        ),
        quality: function (capture) {
            // precedence by length, `em` wins ties:
            return capture[0].length + 0.2;
        },
        parse: function (capture, parse, state) {
            return {
                content: parse(capture[2] || capture[1], state),
            };
        },
        react: function (node, output, state) {
            return reactElement("em", state.key, {
                children: output(node.content, state),
            });
        },
        html: function (node, output, state) {
            return htmlTag("em", output(node.content, state));
        },
    },
    strong: {
        order: currOrder /* same as em */,
        match: inlineRegex(/^\*\*((?:\\[\s\S]|[^\\])+?)\*\*(?!\*)/),
        quality: function (capture) {
            // precedence by length, wins ties vs `u`:
            return capture[0].length + 0.1;
        },
        parse: parseCaptureInline,
        react: function (node, output, state) {
            return reactElement("strong", state.key, {
                children: output(node.content, state),
            });
        },
        html: function (node, output, state) {
            return htmlTag("strong", output(node.content, state));
        },
    },
    u: {
        order: currOrder++ /* same as em&strong; increment for next rule */,
        match: inlineRegex(/^__((?:\\[\s\S]|[^\\])+?)__(?!_)/),
        quality: function (capture) {
            // precedence by length, loses all ties
            return capture[0].length;
        },
        parse: parseCaptureInline,
        react: function (node, output, state) {
            return reactElement("u", state.key, {
                children: output(node.content, state),
            });
        },
        html: function (node, output, state) {
            return htmlTag("u", output(node.content, state));
        },
    },
    del: {
        order: currOrder++,
        match: inlineRegex(
            /^~~(?=\S)((?:\\[\s\S]|~(?!~)|[^\s~\\]|\s(?!~~))+?)~~/,
        ),
        parse: parseCaptureInline,
        react: function (node, output, state) {
            return reactElement("del", state.key, {
                children: output(node.content, state),
            });
        },
        html: function (node, output, state) {
            return htmlTag("del", output(node.content, state));
        },
    },
    inlineCode: {
        order: currOrder++,
        match: inlineRegex(/^(`+)([\s\S]*?[^`])\1(?!`)/),
        parse: function (capture, _parse, _state) {
            return {
                content: capture[2].replace(
                    INLINE_CODE_ESCAPE_BACKTICKS_R,
                    "$1",
                ),
            };
        },
        react: function (node, _output, state) {
            return reactElement("code", state.key, {
                children: node.content,
            });
        },
        html: function (node, _output, _state) {
            return htmlTag("code", sanitizeText(node.content));
        },
    },
    br: {
        order: currOrder++,
        match: anyScopeRegex(/^ {2,}\n/),
        parse: ignoreCapture,
        react: function (_node, _output, state) {
            return reactElement("br", state.key, EMPTY_PROPS);
        },
        html: function (_node, _output, _state) {
            return "<br>";
        },
    },
    text: {
        order: currOrder++,
        // Here we look for anything followed by non-symbols,
        // double newlines, or double-space-newlines
        // We break on any symbol characters so that this grammar
        // is easy to extend without needing to modify this regex
        match: anyScopeRegex(
            /^[\s\S]+?(?=[^0-9A-Za-z\s\u00c0-\uffff]|\n\n| {2,}\n|\w+:\S|$)/,
        ),
        parse: function (capture, _parse, _state) {
            return {
                content: capture[0],
            };
        },
        react: function (node, _output, _state) {
            return node.content;
        },
        html: function (node, _output, _state) {
            return sanitizeText(node.content);
        },
    },
};

/** (deprecated) */
const ruleOutput = function <Rule>(
    rules: OutputRules<Rule>,
    property: keyof Rule,
) {
    if (!property && typeof console !== "undefined") {
        console.warn(
            "simple-markdown ruleOutput should take 'react' or " +
            "'html' as the second argument.",
        );
    }

    const nestedRuleOutput = function (
        ast: SingleASTNode | UnTypedASTNode,
        outputFunc: Output<any>,
        state: State,
    ) {
        // @ts-expect-error - TS2349 - This expression is not callable.
        //   Type 'unknown' has no call signatures.
        return rules[ast.type][property](ast, outputFunc, state);
    };
    return nestedRuleOutput;
};

/** (deprecated)
 */
const reactFor = function (outputFunc: ReactNodeOutput): ReactOutput {
    const nestedOutput: ReactOutput = function (ast, state) {
        state = state || {};
        if (Array.isArray(ast)) {
            const oldKey = state.key;
            const result: Array<ReactElements> = [];

            // map nestedOutput over the ast, except group any text
            // nodes together into a single string output.
            let lastResult = null;
            for (let i = 0; i < ast.length; i++) {
                state.key = "" + i;
                const nodeOut = nestedOutput(ast[i], state);
                if (
                    typeof nodeOut === "string" &&
                    typeof lastResult === "string"
                ) {
                    //// @ts-expect-error - TS2322 - Type 'string' is not assignable to type 'null'.
                    lastResult = lastResult + nodeOut;
                    result[result.length - 1] = lastResult;
                } else {
                    result.push(nodeOut);
                    //// @ts-expect-error - TS2322 - Type 'ReactNode' is not assignable to type 'null'.
                    lastResult = nodeOut;
                }
            }

            state.key = oldKey;
            return result;
        } else {
            return outputFunc(ast, nestedOutput, state);
        }
    };
    return nestedOutput;
};

/** (deprecated)
 */
const htmlFor = function (outputFunc: HtmlNodeOutput): HtmlOutput {
    const nestedOutput: HtmlOutput = function (ast, state) {
        state = state || {};
        if (Array.isArray(ast)) {
            return ast
                .map(function (node) {
                    return nestedOutput(node, state);
                })
                .join("");
        } else {
            return outputFunc(ast, nestedOutput, state);
        }
    };
    return nestedOutput;
};

const outputFor = function <Rule>(
    rules: OutputRules<Rule>,
    property: keyof Rule,
    defaultState: State | null = {},
) {
    if (!property) {
        throw new Error(
            "simple-markdown: outputFor: `property` must be " +
            "defined. " +
            "if you just upgraded, you probably need to replace `outputFor` " +
            "with `reactFor`",
        );
    }

    let latestState: State;
    const arrayRule: ArrayRule = rules.Array || defaultRules.Array;

    // Tricks to convince tsc that this const is not null:
    // @ts-expect-error - TS2538 - Type 'symbol' cannot be used as an index type.
    const arrayRuleCheck = arrayRule[property];
    if (!arrayRuleCheck) {
        throw new Error(
            "simple-markdown: outputFor: to join nodes of type `" +
            // @ts-expect-error - TS2469 - The '+' operator cannot be applied to type 'symbol'.
            property +
            "` you must provide an `Array:` joiner rule with that type, " +
            "Please see the docs for details on specifying an Array rule.",
        );
    }
    const arrayRuleOutput = arrayRuleCheck;

    const nestedOutput: Output<any> = function (ast, state) {
        state = state || latestState;
        latestState = state;
        if (Array.isArray(ast)) {
            return arrayRuleOutput(ast, nestedOutput, state);
        } else {
            // @ts-expect-error - TS2349 - This expression is not callable.
            //   Type 'unknown' has no call signatures.
            return rules[ast.type][property](ast, nestedOutput, state);
        }
    };

    const outerOutput: Output<any> = function (ast, state) {
        latestState = populateInitialState(state, defaultState);
        return nestedOutput(ast, latestState);
    };
    return outerOutput;
};

// @ts-expect-error - TS2345 - Argument of type 'DefaultRules' is not assignable to parameter of type 'ParserRules'.
const defaultRawParse = parserFor(defaultRules);

const defaultBlockParse = function (
    source: string,
    state?: State | null,
): Array<SingleASTNode | UnTypedASTNode> {
    state = state || {};
    state.inline = false;
    return defaultRawParse(source, state);
};

const defaultInlineParse = function (
    source: string,
    state?: State | null,
): Array<SingleASTNode | UnTypedASTNode> {
    state = state || {};
    state.inline = true;
    return defaultRawParse(source, state);
};

const defaultImplicitParse = function (
    source: string,
    state?: State | null,
): Array<SingleASTNode | UnTypedASTNode> {
    const isBlock = BLOCK_END_R.test(source);
    state = state || {};
    state.inline = !isBlock;
    return defaultRawParse(source, state);
};

const defaultReactOutput: ReactOutput = outputFor(defaultRules, "react");
const defaultHtmlOutput: HtmlOutput = outputFor(defaultRules, "html");

const markdownToReact = function (
    source: string,
    state?: State | null,
): ReactElements {
    return defaultReactOutput(defaultBlockParse(source, state), state);
};

const markdownToHtml = function (source: string, state?: State | null): string {
    return defaultHtmlOutput(defaultBlockParse(source, state), state);
};

// TODO: This needs definition
// type Props = any;
// changes by @oriel-beck from not typed to any
const ReactMarkdown = function (props: any): React.ReactElement {
    const divProps: Record<string, any> = {};

    for (const prop in props) {
        if (
            prop !== "source" &&
            Object.prototype.hasOwnProperty.call(props, prop)
        ) {
            divProps[prop] = props[prop];
        }
    }
    divProps.children = markdownToReact(props.source);

    return reactElement("div", null, divProps);
};

type Exports = {
    readonly defaultRules: DefaultRules;
    readonly parserFor: (
        rules: ParserRules,
        defaultState?: State | null | undefined,
    ) => Parser;
    readonly outputFor: <Rule>(
        rules: OutputRules<Rule>,
        param: keyof Rule,
        defaultState?: State | null | undefined,
    ) => Output<any>;
    readonly ruleOutput: <Rule>(
        rules: OutputRules<Rule>,
        param: keyof Rule,
    ) => NodeOutput<any>;
    readonly reactFor: (arg1: ReactNodeOutput) => ReactOutput;
    readonly htmlFor: (arg1: HtmlNodeOutput) => HtmlOutput;
    readonly inlineRegex: (regex: RegExp) => MatchFunction;
    readonly blockRegex: (regex: RegExp) => MatchFunction;
    readonly anyScopeRegex: (regex: RegExp) => MatchFunction;
    readonly parseInline: (
        parse: Parser,
        content: string,
        state: State,
    ) => ASTNode;
    readonly parseBlock: (
        parse: Parser,
        content: string,
        state: State,
    ) => ASTNode;
    readonly markdownToReact: (
        source: string,
        state?: State | null | undefined,
    ) => ReactElements;
    readonly markdownToHtml: (
        source: string,
        state?: State | null | undefined,
    ) => string;
    readonly ReactMarkdown: (props: {
        source: string;
        [key: string]: any;
    }) => ReactElement;
    readonly defaultRawParse: (
        source: string,
        state?: State | null | undefined,
    ) => Array<SingleASTNode | UnTypedASTNode>;
    readonly defaultBlockParse: (
        source: string,
        state?: State | null | undefined,
    ) => Array<SingleASTNode | UnTypedASTNode>;
    readonly defaultInlineParse: (
        source: string,
        state?: State | null | undefined,
    ) => Array<SingleASTNode | UnTypedASTNode>;
    readonly defaultImplicitParse: (
        source: string,
        state?: State | null | undefined,
    ) => Array<SingleASTNode | UnTypedASTNode>;
    readonly defaultReactOutput: ReactOutput;
    readonly defaultHtmlOutput: HtmlOutput;
    readonly preprocess: (source: string) => string;
    readonly sanitizeText: (text: Attr) => string;
    readonly sanitizeUrl: (
        url?: string | null | undefined,
    ) => string | null | undefined;
    readonly unescapeUrl: (url: string) => string;
    readonly htmlTag: (
        tagName: string,
        content: string,
        attributes?:
            | Partial<Record<any, Attr | null | undefined>>
            | null
            | undefined,
        isClosed?: boolean | null | undefined,
    ) => string;
    readonly reactElement: (
        type: string,
        key: string | null,
        props: {
            [key: string]: any;
        },
    ) => ReactElement;
    /**
     * defaultParse is deprecated, please use `defaultImplicitParse`
     * @deprecated
     */
    readonly defaultParse: (...args: any[]) => any;
    /**
     * defaultOutput is deprecated, please use `defaultReactOutput`
     * @deprecated
     */
    readonly defaultOutput: (...args: any[]) => any;
};

export type {
    // Hopefully you shouldn't have to use these, but they're here if you need!
    // Top-level API:
    State,
    Parser,
    Output,
    ReactOutput,
    HtmlOutput,
    // Most of the following types should be considered experimental and
    // subject to change or change names. Again, they shouldn't be necessary,
    // but if they are I'd love to hear how so I can better support them!

    // Individual Rule fields:
    Capture,
    MatchFunction,
    ParseFunction,
    NodeOutput,
    ArrayNodeOutput,
    ReactNodeOutput,
    // Single rules:
    ParserRule,
    ReactOutputRule,
    HtmlOutputRule,
    // Sets of rules:
    ParserRules,
    OutputRules,
    Rules,
    ReactRules,
    HtmlRules,
    SingleASTNode,
};

const SimpleMarkdown: Exports = {
    defaultRules: defaultRules,
    parserFor: parserFor,
    outputFor: outputFor,

    inlineRegex: inlineRegex,
    blockRegex: blockRegex,
    anyScopeRegex: anyScopeRegex,
    parseInline: parseInline,
    parseBlock: parseBlock,

    // default wrappers:
    markdownToReact: markdownToReact,
    markdownToHtml: markdownToHtml,
    ReactMarkdown: ReactMarkdown,

    defaultBlockParse: defaultBlockParse,
    defaultInlineParse: defaultInlineParse,
    defaultImplicitParse: defaultImplicitParse,

    defaultReactOutput: defaultReactOutput,
    defaultHtmlOutput: defaultHtmlOutput,

    preprocess: preprocess,
    sanitizeText: sanitizeText,
    sanitizeUrl: sanitizeUrl,
    unescapeUrl: unescapeUrl,
    htmlTag: htmlTag,
    reactElement: reactElement,

    // deprecated:
    defaultRawParse: defaultRawParse,
    ruleOutput: ruleOutput,
    reactFor: reactFor,
    htmlFor: htmlFor,

    defaultParse: function (...args) {
        if (typeof console !== "undefined") {
            console.warn(
                "defaultParse is deprecated, please use `defaultImplicitParse`",
            );
        }
        // @ts-expect-error lib error
        // eslint-disable-next-line prefer-spread
        return defaultImplicitParse.apply(null, args);
    },
    defaultOutput: function (...args) {
        if (typeof console !== "undefined") {
            console.warn(
                "defaultOutput is deprecated, please use `defaultReactOutput`",
            );
        }
        // @ts-expect-error lib error
        // eslint-disable-next-line prefer-spread
        return defaultReactOutput.apply(null, args);
    },
};

export default SimpleMarkdown;