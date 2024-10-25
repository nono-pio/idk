export default function CustomForm({title, children, onSubmit}) {
    return (
        <div className="px-4 py-3 my-3 d-flex flex-column align-items-center">
            <h1 className="display-5 fw-bold text-body-emphasis mb-4 text-wrap text-center" style={{maxWidth:780}}>{ title }</h1>
            <div className="d-flex flex-column w-100" style={{maxWidth:780}}>
                { children }
                <button type="button" className="btn btn-primary btn-lg px-4 gap-3 align-self-center" onClick={onSubmit}>Enter</button>
            </div>
        </div>
    )
}