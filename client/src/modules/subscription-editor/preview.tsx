import { useFormikContext } from "formik";
import type { FormikInitialValue, EmbedField, EmbedFormikInitialValue } from "../../types/types";
import { EventType } from "../../util/enums";
import DiscordMarkdown from "../../components/markdown/markdown";

const getDate = () => new Date().toLocaleString('en', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: true
})

export default function EmbedPreview({ eventType }: { eventType: EventType }) {
    const formik = useFormikContext<FormikInitialValue>();
    const data = formik.values.events[eventType.toString()].message

    return (
        <div className="relative pr-5">
            {/* pfp */}
            <div className="absolute top-2 left-2">
                <img className="w-10 h-10 rounded-full" src={data.avatar} alt="Avatar" />
            </div>
            <div className="ml-16 pt-2">
                {/* Username, date */}
                <div className="flex items-center">
                    <span className="text-lg">{data.username}</span>
                    <span className="mx-1 flex justify-center rounded h-[1rem] w-7 items-center pb-0.5 bg-[#5865f2]" style={{ fontSize: '11px' }}>BOT</span>
                    <span className="mt-1 ml-2 text-xs text-[#949ba4]">Today at {getDate()}</span>
                </div>
                {data.content &&
                    <div className="overflow-hidden max-w-full pb-2">
                        <div className="max-w-full break-words whitespace-pre-line text-pretty">
                            <DiscordMarkdown>
                                {data.content}
                            </DiscordMarkdown>
                        </div>
                    </div>
                }
                {/* TODO: demo image */}
                {data?.embeds.map((embed) => (
                    <Embed key={embed.id as string} embed={embed} />
                ))}
            </div>
        </div>

    )
}

function Embed({ embed }: { embed: EmbedFormikInitialValue, }) {

    const MAX_FIELDS_PER_ROW = 3;
    const FIELD_GRID_SIZE = 12;

    const getFieldGridColumn = (embed: EmbedFormikInitialValue, field: EmbedField): string => {
        const fieldIndex = embed.fields.indexOf(field);

        if (!field.inline) return `1 / ${FIELD_GRID_SIZE + 1}`;

        let startingField = fieldIndex;
        while (startingField > 0 && embed.fields[startingField - 1].inline) {
            startingField--;
        }

        let totalInlineFields = 0;
        for (let i = startingField; i < embed.fields.length && embed.fields[i].inline; i++) {
            totalInlineFields++;
        }

        const indexInSequence = fieldIndex - startingField;
        const currentRow = Math.floor(indexInSequence / MAX_FIELDS_PER_ROW);
        const indexOnRow = indexInSequence % MAX_FIELDS_PER_ROW;
        const totalOnLastRow = totalInlineFields % MAX_FIELDS_PER_ROW || MAX_FIELDS_PER_ROW;
        const fullRows = Math.floor((totalInlineFields - totalOnLastRow) / MAX_FIELDS_PER_ROW);
        const totalOnRow = currentRow >= fullRows ? totalOnLastRow : MAX_FIELDS_PER_ROW;

        const columnSpan = FIELD_GRID_SIZE / totalOnRow;
        const start = indexOnRow * columnSpan + 1;
        const end = start + columnSpan;

        return `${start} / ${end}`;
    };


    return (
        <div className="flex mb-5 flex-col max-w-[432px] bg-[#2b2d31] px-4 py-3 rounded-sm border-l-4" style={{ borderColor: embed.color || '#202225' }}>
            {/* Top */}
            <div className="flex">
                {/* Author, title, description, image */}
                <div>
                    <div className="flex">
                        <div className="flex flex-col">
                            {/* Author */}
                            {embed.author?.text &&
                                <div className="flex items-center space-x-2">
                                    {embed.author.icon &&
                                        <img className="w-6 h-6 rounded-full object-contain" src={embed.author.icon} alt="" />
                                    }
                                    {embed.author.url
                                        ?
                                        <a target="_blank" className="text-[14px] font-semibold mb-1 hover:underline w-fit" href={embed.author.url}>{embed.author.text}</a>
                                        :
                                        <span className="text-[14px] font-semibold w-fit">{embed.author.text}</span>
                                    }
                                </div>
                            }
                            {/* Title */}
                            {embed.title?.text &&
                                <>
                                    {embed.title.url
                                        ?
                                        <a target="_blank" className="text-[22px] font-semibold text-[#00a8fc] hover:underline w-fit" href={embed.title.url}>{embed.title.text}</a>
                                        :
                                        <span className="text-[22px] font-semibold w-fit">{embed.title.text}</span>
                                    }
                                </>
                            }
                            {/* Description */}
                            {embed.description &&
                                <div className="mt-[8px] text-sm break-words whitespace-pre-line text-pretty">
                                    <DiscordMarkdown>
                                        {embed.description}
                                    </DiscordMarkdown>
                                </div>
                            }
                        </div>
                        <span className="flex-auto"></span>
                        {/* Thumbnail */}
                        {embed.thumbnail &&
                            <img className="max-w-[80px] max-h-[80px] rounded object-contain self-start" src={embed.thumbnail} alt="" />
                        }
                    </div>
                    {!!embed.fields.length &&
                        <div className="grid gap-1 mt-[8px] text-sm" style={{ gridColumn: '1/2' }}>
                            {embed.fields.map((field) => (
                                <div key={field.id as string} className="flex flex-col" style={{ gridColumn: getFieldGridColumn(embed, field) }}>
                                    <h3 className="font-semibold">{field.name}</h3>
                                    <div className="font-normal">
                                        <DiscordMarkdown>
                                            {field.value}
                                        </DiscordMarkdown>
                                    </div>
                                </div>
                            ))}
                        </div>
                    }
                    {/* Image */}
                    {embed.image &&
                        <img className="mt-[16px] max-h-[300px] max-w-[400px] rounded object-contain" src={embed.image} alt="Embed Image" />
                    }
                </div>
            </div>
            {/* Bottom (footer) */}
            {embed.footer?.text &&
                <div className="flex items-center space-x-2 mt-2">
                    {(embed.footer.icon && (embed.addTimestamp || embed.footer.text)) &&
                        <img className="rounded-full w-6 h-6 object-contain" src={embed.footer.icon} alt="Footer icon" />
                    }
                    <span>{embed.footer.text}</span>
                    {embed.addTimestamp &&
                        <span> • Today at {getDate()}</span>
                    }
                </div>
            }
        </div>
    )
}
