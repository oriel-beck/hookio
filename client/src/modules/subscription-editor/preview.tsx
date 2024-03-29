import { useFormikContext } from "formik";
import type { Embed as EmbedType, EmbedFormInitialValues, EmbedField } from "../../types/types";

const getDate = () => new Date().toLocaleString('en', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: true
})

export default function EmbedPreview() {
    const formik = useFormikContext<EmbedFormInitialValues>();
    const fallbacks = {
        username: "Hookio",
        avatar: "https://c8.alamy.com/comp/R1PP58/hook-vector-icon-isolated-on-transparent-background-hook-transparency-logo-concept-R1PP58.jpg"
    }

    return (
        <div className="relative pr-5">
            {/* pfp */}
            <div className="absolute top-2 left-2">
                <img className="w-10 h-10 rounded-full" src={formik.values.avatar || fallbacks.avatar} alt="Avatar" />
            </div>
            <div className="ml-16 pt-2">
                {/* Username, date */}
                <div className="flex items-center">
                    <span className="text-lg">{formik.values.username || fallbacks.username}</span>
                    <span className="mx-1 flex justify-center rounded h-[1rem] w-7 items-center pb-0.5 bg-[#5865f2]" style={{ fontSize: '11px' }}>BOT</span>
                    <span className="mt-1 ml-2 text-xs text-[#949ba4]">Today at {getDate()}</span>
                </div>
                {formik.values.content &&
                    <div className="overflow-hidden max-w-full pb-2">
                        <p className="max-w-full break-words whitespace-pre-line text-pretty">{formik.values.content}</p>
                    </div>
                }
                {/* TODO: demo image */}
                {formik.values?.embeds.map((embed) => (
                    <Embed embed={embed} />
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
        <div className="flex mb-5 flex-col max-w-[432px] bg-[#2b2d31] w-fit px-4 py-3 rounded-sm border-l-4" style={{ borderColor: embed.color }}>
            {/* Top */}
            <div className="flex">
                {/* Author, title, description, image */}
                <div className="max-w-[300px]">
                    {/* Author */}
                    {embed.author &&
                        <div className="flex items-center space-x-2 mb-1">
                            {embed.authorIcon &&
                                <img className="w-6 h-6 rounded-full" src={embed.authorIcon} alt="" />
                            }
                            {embed.authorUrl
                                ?
                                <a className="font-semibold mb-1 hover:underline w-fit" href={embed.authorUrl}>{embed.author}</a>
                                :
                                <span className="font-semibold mb-1 w-fit">{embed.author}</span>
                            }
                        </div>
                    }
                    {/* Title */}
                    {embed.title &&
                        <div className="mb-1 mt-1 w-fit">
                            {embed.titleUrl
                                ?
                                <a className="font-semibold text-[#00a8fc] hover:underline w-fit" href={embed.titleUrl}>{embed.title}</a>
                                :
                                <span className="font-semibold w-fit">{embed.title}</span>
                            }
                        </div>
                    }
                    {/* Description */}
                    {embed.description &&
                        <p className="break-words whitespace-pre-line text-pretty">{embed.description}</p>
                    }
                    {!!embed.fields.length &&
                        <div className="grid gap-1 my-4" style={{ gridColumn: '1/2' }}>
                            {embed.fields.map((field) => (
                                <div key={field.id} className="flex flex-col" style={{ gridColumn: getFieldGridColumn(embed, field) }}>
                                    <h3 className="font-semibold">{field.name}</h3>
                                    <p>{field.value}</p>
                                </div>
                            ))}
                        </div>
                    }
                    {/* Image */}
                    {embed.image &&
                        <img className="max-h-[300px] mx-w-[400px] rounded" src={embed.image} alt="Embed Image" />
                    }
                </div >
                {/* Thumbnail */}
                {embed.thumbnail &&
                    <img className="max-w-[80px] max-h-[80px] rounded" src={embed.thumbnail} alt="" />
                }
            </div>
            {/* Bottom (footer) */}
            {embed.footer &&
                <div className="flex items-center space-x-2 mt-2">
                    {(embed.footerIcon && (embed.addTimestamp || embed.footer)) &&
                        <img className="rounded-full w-6 h-6" src={embed.footerIcon} alt="Footer icon" />
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
