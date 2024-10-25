export default function ResultCard({ result }) {
    return (
        <div className="card mb-3">
            <div className="card-header">
                {result.domain}
            </div>
            <div className="card-body">
                <h5 className="card-title">{result.title}</h5>
                <p className="card-text">{result.content}</p>
            </div>
        </div>
    )
}