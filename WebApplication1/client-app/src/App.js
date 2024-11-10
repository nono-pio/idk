import React from 'react';
import {addStyles, EditableMathField, StaticMathField} from 'react-mathquill'
import 'bootstrap/dist/css/bootstrap.min.css';
import Header from './Header'
import NavBar from "./NavBar";
import {useState} from "react";
import EvalForm from "./forms/EvalForm";
import Footer from "./Footer";

import ResultsContainer from "./ResultsContainer";
import AnalyseForm from "./forms/AnalyseForm";
import DerivativeForm from "./forms/DerivativeForm";
import IntegralForm from "./forms/IntegralForm";
import EquationForm from "./forms/EquationForm";
import MathExpr from "./components/MathExpr";
import MathInput from "./components/MathInput";
import LimitForm from "./forms/LimitForm";

addStyles()

const sections = [
    {
        name: "Simplify",
        shortName: "eval",
        description: "Evaluate and Simplify an expression"
    },
    {
        name: "Analyse",
        shortName: "analyse",
        description: "Analyse a fonction, find domain, range, derive, ..."
    },
    {
        name: "Solver",
        shortName: "equation",
        description: "Solve an equation"
    }
]

const otherSections = [
    {
        name: "Derive",
        shortName: "ddx",
        description: "Derive a fonction"
    },
    {
        name: "Integrate",
        shortName: "integral",
        description: "Integrate a fonction"
    },
    {
        name: "Limit",
        shortName: "limit",
        description: "Limit a fonction to a point"
    }
]

function App() {

    const [activeForm, _setActiveForm] = useState("eval")
    const [results, setResults] = useState(undefined)
    
    const setActiveForm = (section) => {
        if (section !== activeForm) {
            _setActiveForm(section)
            setResults(undefined)
        }
    }
    
    const forms = {
        "eval": <EvalForm setResults={setResults}/>,
        "analyse": <AnalyseForm setResults={setResults}/>,
        "ddx": <DerivativeForm setResults={setResults}/>,
        "integral": <IntegralForm setResults={setResults}/>,
        "equation": <EquationForm setResults={setResults}/>,
        "limit": <LimitForm setResults={setResults} />
    }

    return (
        <>
            <Header/>
            <NavBar 
                sections={sections} 
                otherSections={otherSections} 
                activeSection={activeForm}
                setActiveSection={setActiveForm}
            />

            {forms[activeForm] ?? `No Form is corresponding to ${activeForm}`}

            <ResultsContainer results={results}/>

            <Footer/>
        </>
    );
}

export default App;
