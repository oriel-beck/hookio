import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";

export default function Login() {
    const navigate = useNavigate();
    const [params] = useSearchParams();
    const [modifiedParams, setModifiedParams] = useState(params);
    useEffect(() => {
        async function validateUser() {
            const validated = await fetch(`/api/users/authenticate/${params.get("code")}`, { method: "POST" }).then((r) => r.json());
            if (validated) {
                console.log("validated")
                setModifiedParams(new URLSearchParams());
                navigate("/servers", { replace: true });
                // redirect to servers
            } else {
                console.log("failed")
                // redirect to login without code
            }
        }
        if (modifiedParams.get("code")) validateUser();
    }, [modifiedParams, navigate, params])

    return (
        <>
            {params.get("code")
                ?
                <>Validating code, please wait...</>
                :
                <>Please log in</>
            }
        </>
    )
}