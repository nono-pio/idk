import CustomForm from "./CustomForm";
import MathInput from "../components/MathInput";
import { useState } from "react";
import Fetch from "../api/FetchAPI";
import MathExpr from "../components/MathExpr";

export default function EquationForm({ setResults }) {
    async function onSubmit() {

        if (lhs === "" || rhs === "" || variable === "")
            return

        let results = await Fetch("equation", {lhs, rhs, var: variable})
        if (results === null)
            setResults([ ])
        else
            setResults([
                {
                    domain: "Equation",
                    title: <>Solutions of <MathExpr latex={lhs + " = " + rhs} /></>,
                    content: <MathExpr latex={results.solutions} />
                },
            ])
    }

    const [lhs, setLhs] = useState("")
    const [rhs, setRhs] = useState("")
    const [variable, setVariable] = useState("x")

    return (
        <CustomForm title="Equation Solver" onSubmit={onSubmit}>
            <label>Enter the equation to solve</label>
            <div id="equation" className="mb-3 d-flex gap-1 align-items-center">
                <MathInput className="flex-grow-1" setLatex={setLhs} />
                <span className="flex-grow-0">=</span>
                <MathInput className="flex-grow-1" setLatex={setRhs}></MathInput>
            </div>
            <label>Enter the variable</label>
            <MathInput className="mb-3" setLatex={setVariable} latex={variable} />
        </CustomForm>
    )
}