import React from 'react';
import { addStyles, EditableMathField, StaticMathField } from 'react-mathquill'
import 'bootstrap/dist/css/bootstrap.min.css';
import Header from './Header'
import NavBar from "./NavBar";
import {useState} from "react";
import EvalForm from "./forms/EvalForm";
import Footer from "./Footer";
import ResultsContainer from "./ResultsContainer";

addStyles()

const sections = [
    {
        name: "Evaluate an expression",
        shortName: "eval",
        description: "Evaluate and Simplify an expression"
    },
    {
        name: "Analyse a function",
        shortName: "analyse",
        description: "Analyse a fonction, find domain, range, derive, ..."
    }, 
    {
        name: "Solve an equation",
        shortName: "equation",
        description: "Solve an equation"
    }
]

const otherSections = [
    {
        name: "Derive a function",
        shortName: "ddx",
        description: "Derive a fonction"
    },
    {
        name: "Integrate a function",
        shortName: "integral",
        description: "Integrate a fonction"
    }
]

function App() {
    
    const [ activeForm, setActiveForm ] = useState("eval")
    const [ results, setResults ] = useState(undefined)
    
    const forms = {
        "eval" : <EvalForm setResults={setResults} />
    }
    
    
    
  return (
    <>
        <Header />
        <NavBar sections={sections} otherSections={otherSections} activeSection={activeForm} setActiveSection={setActiveForm} />

        { forms[activeForm] ?? `No Form is corresponding to ${activeForm}` }
        
        <ResultsContainer results={results} />
        
        <Footer />
    </>
  );
}

/*

<div className="px-4 py-3 my-3 text-center" data-section="ddx">
            <h1 className="display-5 fw-bold text-body-emphasis mb-4">Enter your function and variable</h1>
            <div className="d-flex flex-column align-items-center">
                <span className="form-control w-100 mb-2 text-start" style={{maxWidth : 780}} id="ddx-func"></span>
                <input type="text" className="form-control w-100 mb-2" style={{maxWidth : 780}} placeholder="x^2"
                       id="ddx-func-raw"/>
                <input type="text" className="form-control w-100 mb-2" style={{maxWidth : 780}} value="x" id="ddx-variable"/>
                <div className="d-grid gap-2 d-sm-flex justify-content-sm-center">
                    <button type="button" className="btn btn-primary btn-lg px-4 gap-3" onClick="RenderDdx()">Enter</button>
                </div>
            </div>
        </div>
        
        <div className="px-4 py-3 my-3 text-center" data-section="analyse">
            <h1 className="display-5 fw-bold text-body-emphasis mb-4">Enter your function and variable</h1>
            <div className="d-flex flex-column align-items-center">
                <span className="form-control w-100 mb-2 text-start" style={{maxWidth : 780}} id="analyse-func"></span>
                <input type="text" className="form-control w-100 mb-2" style={{maxWidth : 780}} placeholder="x^2"
                       id="analyse-func-raw"/>
                <input type="text" className="form-control w-100 mb-2" style={{maxWidth : 780}} value="x" id="analyse-variable"/>
                <div className="d-grid gap-2 d-sm-flex justify-content-sm-center">
                    <button type="button" className="btn btn-primary btn-lg px-4 gap-3" onClick="RenderAnalyse()">Enter</button>
                </div>
            </div>
        </div>
        
        <div className="px-4 py-3 my-3 text-center" data-section="integral">
            <h1 className="display-5 fw-bold text-body-emphasis mb-4">Enter your function and variable</h1>
            <div className="d-flex flex-column align-items-center">
                <span className="form-control w-100 mb-2 text-start" style={{maxWidth : 780}} id="integral-func"></span>
                <input type="text" className="form-control w-100 mb-2" style={{maxWidth : 780}} placeholder="x^2"
                       id="integral-func-raw"/>
                <input type="text" className="form-control w-100 mb-2" style={{maxWidth : 780}} value="x" id="integral-variable"/>
                <div className="d-grid gap-2 d-sm-flex justify-content-sm-center">
                    <button type="button" className="btn btn-primary btn-lg px-4 gap-3" onClick="RenderIntegral()">Enter</button>
                </div>
            </div>
        </div>
        
        <div className="px-4 py-3 my-3 text-center" data-section="equation">
            <h1 className="display-5 fw-bold text-body-emphasis mb-4">Enter your equation</h1>
            <div className="d-flex flex-column align-items-center">
                <div className="d-flex">
            <span className="form-control w-100 mb-2 text-start flex-grow-1" style={{maxWidth : 390}}
                  id="equation-lhs"></span>
                    <span>=</span>
                    <span className="form-control w-100 mb-2 text-start flex-grow-1" style={{maxWidth : 390}}
                          id="equation-rhs"></span>
                </div>
                <div className="d-flex">
                    <input type="text" className="form-control w-100 mb-2" style={{maxWidth : 390}} placeholder="x^2"
                           id="equation-lhs-raw"/>
                    <span>=</span>
                    <input type="text" className="form-control w-100 mb-2" style={{maxWidth : 390}} placeholder="4"
                           id="equation-rhs-raw"/>
                </div>
                <input type="text" className="form-control w-100 mb-2" style={{maxWidth : 390}} value="x"
                       id="equation-variable"/>
                <div className="d-grid gap-2 d-sm-flex justify-content-sm-center">
                    <button type="button" className="btn btn-primary btn-lg px-4 gap-3" onClick="RenderEquation()">Enter</button>
                </div>
            </div>
        </div>

*/

export default App;
