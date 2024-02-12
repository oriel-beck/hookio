export interface SelectMenuOption {
    value?: string;
    label: string;
}

export default function SelectMenu({ options, label, id, selected }: { options: SelectMenuOption[], label: string, id: string, selected?: string }) {
    // TODO: rewrite this to a custom select menu
    return (
        <div>
            <label htmlFor={id} className="block mb-2 text-sm font-medium text-white">{label}</label>
            <select defaultValue={selected} value={selected} id={id} className="border text-sm rounded-lgblock w-full p-2.5 bg-zinc-600 border-gray-600 placeholder-gray-300 rounded-sm text-white focus:ring-blue-500 focus:border-blue-500 outline-none">
                {options.map((option) => (
                    <option key={option.label} value={option.value}>{option.label}</option>
                ))}
            </select>
        </div>
    )
}