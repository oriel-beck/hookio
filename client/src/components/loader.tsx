import Header from "./header";

export default function Loader() {
    return (
        <>
            <Header showLogin={false} user={null} />
            <main>
                <div className='flex items-center justify-center h-full text-white' style={{ minHeight: '600px' }}>
                    <div
                        className="inline-block h-52 w-52 animate-spin rounded-full border-4 border-solid border-current border-r-transparent align-[-0.125em] motion-reduce:animate-[spin_1.5s_linear_infinite]"
                        role="status">
                        <span
                            className="!absolute !-m-px !h-px !w-px !overflow-hidden !whitespace-nowrap !border-0 !p-0 ![clip:rect(0,0,0,0)]"
                        >Loading...</span
                        >
                    </div>
                </div>
            </main>
        </>
    )
}