export default function Input({ label, id, type }: { label: string, id: string, type: string }) {
    return (
        <div>
            <label htmlFor={id} className="block mb-2 text-sm font-medium text-white">{label}</label>
            <input type={type} id={id} className="border text-sm rounded-lgblock w-full p-2.5 bg-zinc-600 border-gray-600 placeholder-gray-300 rounded-sm text-white focus:ring-blue-500 focus:border-blue-500 outline-none" placeholder="https://discord.com/api/webhooks/..." required />
        </div>
    )
}