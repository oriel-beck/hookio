import { useEffect } from "react";
import { useSearchParams } from "react-router-dom";

export default function Login() {
    const [params] = useSearchParams()

    useEffect(() => {
        async function validateUser() {
            const validated = await fetch(`/api/users/authenticate/${params.get("code")}`, { method: "POST" });
            console.log(validated);
            if (validated) {
                // redirect to servers
            } else {
                // redirect to login without code
            }
        }
        if (params.get("code")) validateUser();
    })

    console.log(params)

    return (
        <>
            {params.get("code")
                ?
                <>Code provided: {params.get("code")}</>
                :
                <>No code provided</>
            }
        </>
    )
}