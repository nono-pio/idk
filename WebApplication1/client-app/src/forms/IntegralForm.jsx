import CustomForm from "./CustomForm";
import MathInput from "../components/MathInput";
import React, { useState } from "react";
import Fetch from "../api/FetchAPI";
import MathExpr from "../components/MathExpr";

export default function IntegralForm({ setResults }) {
    async function onSubmit() {

        if (expression === "")
            return

        let fetch_result = await Fetch("integral", {expr: expression, var: variable, inf:inf === ""?null:inf, sup:sup === ""?null:sup})
        const results = fetch_result.result
        if (results === null)
            setResults([ ])
        else
            setResults([
                {
                    domain: "Integration",
                    title: <>Integral of <MathExpr latex={expression} /></>,
                    content: <MathExpr latex={`${results.expr} = ${results.app}`} />,
                    fetch: fetch_result
                }
            ])
    }

    const [expression, setExpression] = useState("")
    const [variable, setVariable] = useState("x")
    const [inf, setInf] = useState(undefined);
    const [sup, setSup] = useState(undefined);

    return (
        <CustomForm title="Integration" onSubmit={onSubmit}>
            <label>Enter the function to integrate</label>
            <span className="mq-math-mode w-100">
                <span className="mq-root-block mq-hasCursor d-flex align-items-center gap-1">
                    <span className="mq-int mq-non-leaf">
                        <big style={{fontSize:"300%", transform:"scaleX(1)"}}>∫</big>
                        <span className="mq-supsub mq-non-leaf">
                            <span className="mq-sup"><span className="mq-sup-inner" style={{verticalAlign: "2.3em"}}>
                                <MathInput style={{padding:".1rem .15rem"}} setLatex={setInf}/>
                            </span></span>
                            <span className="mq-sub">
                               <MathInput style={{padding:".1rem .15rem"}} setLatex={setSup}/>
                            </span>
                            <span style={{display: "inline-block", width: 0}}>​</span>
                        </span>
                    </span>
                    <MathInput setLatex={setExpression} className="flex-grow-1" style={{padding: ".375rem .75rem"}}/>
                    <var>d</var>
                    <MathInput className="w-auto px-1" style={{minWidth: 25}} latex={variable} setLatex={setVariable}/>
                </span>
            </span>
            {/*<MathInput className="mb-3" setLatex={setExpression} />*/}
            {/*<MathInput className="mb-3" setLatex={setVariable} latex={variable} />*/}
        </CustomForm>
    )
}