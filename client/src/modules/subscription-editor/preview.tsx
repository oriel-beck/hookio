import { useFormikContext } from "formik";
import type { Embed as EmbedType, FormikInitialValue, EmbedField } from "../../types/types";
import { EventType } from "../../util/enums";

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
                        <p className="max-w-full break-words whitespace-pre-line text-pretty">{data.content}</p>
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

function Embed({ embed }: { embed: EmbedType, }) {

    const MAX_FIELDS_PER_ROW = 3
    const FIELD_GRID_SIZE = 12

    const getFieldGridColumn = (embed: EmbedType, field: EmbedField): string => {
        const fieldIndex = embed.fields.indexOf(field)

        if (!field.inline) return `1 / ${FIELD_GRID_SIZE + 1}`

        let startingField = fieldIndex
        while (startingField > 0 && embed.fields[startingField - 1].inline) {
            startingField -= 1
        }

        let totalInlineFields = 0
        while (
            embed.fields.length > startingField + totalInlineFields &&
            embed.fields[startingField + totalInlineFields].inline
        ) {
            totalInlineFields += 1
        }

        const indexInSequence = fieldIndex - startingField
        const currentRow = indexInSequence / MAX_FIELDS_PER_ROW
        const indexOnRow = indexInSequence % MAX_FIELDS_PER_ROW
        const totalOnLastRow =
            totalInlineFields % MAX_FIELDS_PER_ROW || MAX_FIELDS_PER_ROW
        const fullRows = (totalInlineFields - totalOnLastRow) / MAX_FIELDS_PER_ROW
        const totalOnRow =
            currentRow >= fullRows ? totalOnLastRow : MAX_FIELDS_PER_ROW

        const columnSpan = FIELD_GRID_SIZE / totalOnRow
        const start = indexOnRow * columnSpan + 1
        const end = start + columnSpan

        return `${start} / ${end}`
    }


    return (
        <div className="flex mb-5 flex-col max-w-[432px] bg-[#2b2d31] px-4 py-3 rounded-sm border-l-4 " style={{ borderColor: embed.color }}>
            {/* Top */}
            <div className="flex">
                {/* Author, title, description, image */}
                <div>
                    <div className="flex">
                        <div className="flex flex-col">
                            {/* Author */}
                            {embed.author &&
                                <div className="flex items-center space-x-2">
                                    {embed.authorIcon &&
                                        <img className="w-6 h-6 rounded-full object-contain" src={embed.authorIcon} alt="" />
                                    }
                                    {embed.authorUrl
                                        ?
                                        <a target="_blank" className="text-[14px] font-semibold mb-1 hover:underline w-fit" href={embed.authorUrl}>{embed.author}</a>
                                        :
                                        <span className="text-[14px] font-semibold w-fit">{embed.author}</span>
                                    }
                                </div>
                            }
                            {/* Title */}
                            {embed.title &&
                                <>
                                    {embed.titleUrl
                                        ?
                                        <a target="_blank" className="text-[22px] font-semibold text-[#00a8fc] hover:underline w-fit" href={embed.titleUrl}>{embed.title}</a>
                                        :
                                        <span className="text-[22px] font-semibold w-fit">{embed.title}</span>
                                    }
                                </>
                            }
                            {/* Description */}
                            {embed.description &&
                                <p className="mt-[8px] text-sm break-words whitespace-pre-line text-pretty">{embed.description}</p>
                            }
                        </div>
                        <span className="flex-auto"></span>
                        {/* TODO: fix thumbnail placement https://discord.com/channels/745961266149064774/881253551341457509/1223260537169772584
                Maybe add to the same row as author and title and use a spacer between? */}
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
                                    <p className="font-normal">{field.value}</p>
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
            {embed.footer &&
                <div className="flex items-center space-x-2 mt-2">
                    {(embed.footerIcon && (embed.addTimestamp || embed.footer)) &&
                        <img className="rounded-full w-6 h-6 object-contain" src={embed.footerIcon} alt="Footer icon" />
                    }
                    <span>{embed.footer}</span>
                    {embed.addTimestamp &&
                        <span> • Today at {getDate()}</span>
                    }
                </div>
            }
        </div >
    )
}
