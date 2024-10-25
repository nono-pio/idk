import ResultCard from "./ResultCard";

export default function ResultsContainer({ results }) {
    
    if (results?.length === 0)
        results = null
    
    return (
        <div id="results" className="container px-4 py-3 my-3 justify-content-center">
            { results?.map( result => <ResultCard result={result} />) ?? "No result" }
        </div>
    )
}