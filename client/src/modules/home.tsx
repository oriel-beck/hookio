import { useEffect } from "react";
import { useSearchParams } from "react-router-dom"

export default function Home() {
    const [searchParams, setSearchParams] = useSearchParams();

    useEffect(() => {
        // This removes the `code` param so it won't try to re-trigger since used codes are invalid
        const removeCodeParam = () => {
            if (searchParams.has("code")) {
                searchParams.delete("code");
                setSearchParams(searchParams);
            }
        }
        removeCodeParam()
    });

    return (
        <div className="text-white flex-auto">
            This is home
        </div>
    )
}