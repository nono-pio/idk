import CustomForm from "./CustomForm";
import MathInput from "../components/MathInput";
import { useState } from "react";
import Fetch from "../api/FetchAPI";
import MathExpr from "../components/MathExpr";

const titleMapping = {
    evalFunc: "Evaluation",
    domain: "Domain",
    range: "Range",
    derivative: "Derivation",
    integral: "Integral",
    reciprocal: "Reciprocal",
    seriesExpansion: "Series Expansion",
    factorization: "Factorisation"
}

export default function AnalyseForm({ setResults }) {
    async function onSubmit() {

        if (expression === "" || variable === "")
            return

        let results = await Fetch("analyse", {expr: expression, var: variable})
        if (results === null)
            setResults([ ])
        else
            setResults(
                Object.entries(results)
                    .filter(([_, result]) => result !== null && result !== "" && result !== "NaN")
                    .map(([op, result]) => {
                        return {
                            domain: "Analyse",
                            title: titleMapping[op],
                            content: <MathExpr latex={result} />
                        }
                    })
            )
    }

    const [expression, setExpression] = useState("")
    const [variable, setVariable] = useState("x")

    return (
        <CustomForm title="Function Analyser" onSubmit={onSubmit}>
            <label>Enter the function to analyse</label>
            <MathInput className="mb-3" setLatex={setExpression} />
            <label>Enter the variable</label>
            <MathInput className="mb-3" setLatex={setVariable} latex={variable} />
        </CustomForm>
    )
}