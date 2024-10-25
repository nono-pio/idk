import Nav from 'react-bootstrap/Nav';
import NavDropdown from 'react-bootstrap/NavDropdown';
import {OverlayTrigger, Tooltip} from "react-bootstrap";

function NavBar({sections, otherSections, activeSection, setActiveSection}) {

    return (
        <Nav variant="tabs" activeKey={activeSection} onSelect={section => setActiveSection(section)}>

            {sections.map(section => (
                <OverlayTrigger key={section.name} overlay={<Tooltip id={section.name}>{section.description}</Tooltip>}>
                    <Nav.Item>
                        <Nav.Link eventKey={section.shortName}>{section.name}</Nav.Link>
                    </Nav.Item>
                </OverlayTrigger>
            ))}

            <NavDropdown title="Others">
                {/*{ Object.values(Object.groupBy(otherSections, section => section.type))*/}
                {/*    .map(sections => sections.map(section => section.s).join("+")).join(<li>
                        <hr className="dropdown-divider"/>
                    </li>) }*/}
                {otherSections.map(section => (
                    <OverlayTrigger key={section.name} overlay={<Tooltip id={section.name}>{section.description}</Tooltip>}>
                        <NavDropdown.Item eventKey={section.shortName}>
                            {section.name}
                        </NavDropdown.Item>
                    </OverlayTrigger>
                ))}
            </NavDropdown>
        </Nav>
    )
}

export default NavBar;