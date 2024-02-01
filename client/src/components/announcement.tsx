import type { Announcement as AnnouncementType } from "../types/types";

export default function Announcement({ announcement }: { announcement: AnnouncementType }) {
    const colors = getHeaderHex(announcement.announcementType);
    return (
        <div className="rounded-sm" style={{ backgroundColor: colors.body }}>
            <div className="p-3 rounded-t-sm font-bold text-lg" style={{ backgroundColor: colors.header }}>
                <h3>{announcement.origin}</h3>
            </div>
            <div className="flex justify-end">
                <button className="bg-slate-500 hover:bg-slate-700 text-white font-bold py-2 px-4 rounded m-2 w-full">Edit</button>
            </div>
        </div>
    )
}

function getHeaderHex(announcementType: number) {
    // TODO: add colors for all announcement types
    switch (announcementType) {
        case 0:
            return {
                header: '#FF0000',
                body: '#282828'
            };
        default:
            return {
                header: 'rgb(39 39 42 / var(--tw-bg-opacity))',
                body: 'rgb(113 113 122 / var(--tw-bg-opacity))'
            }
    }
}