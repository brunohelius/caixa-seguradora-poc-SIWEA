#!/usr/bin/env python3
"""
Content Extractor for Visual Age Migration PDF Generation
Extracts content from source specification files for PDF document generation
"""

import os
import re
import yaml
import json
from pathlib import Path
from typing import Dict, List, Any, Optional


class ContentExtractor:
    """Extracts structured content from markdown specification files"""

    def __init__(self, source_dir: str):
        """Initialize the content extractor with source directory"""
        self.source_dir = Path(source_dir)
        self.spec_file = self.source_dir / "spec.md"
        self.plan_file = self.source_dir / "plan.md"
        self.research_file = self.source_dir / "research.md"
        self.data_model_file = self.source_dir / "data-model.md"

        # Cache for loaded content
        self._cache: Dict[str, str] = {}

    def _load_file(self, file_path: Path) -> str:
        """Load and cache file content"""
        if str(file_path) not in self._cache:
            with open(file_path, 'r', encoding='utf-8') as f:
                self._cache[str(file_path)] = f.read()
        return self._cache[str(file_path)]

    def extract_user_stories(self) -> List[Dict[str, Any]]:
        """Extract user stories from spec.md"""
        content = self._load_file(self.spec_file)
        stories = []

        # Pattern to match user stories
        pattern = r'### User Story (\d+) - ([^(]+)\(Priority: (P\d+)\)\n\n(.*?)\n\n\*\*Why this priority\*\*: (.*?)\n\n\*\*Independent Test\*\*: (.*?)\n\n\*\*Acceptance Scenarios\*\*:(.*?)(?=---|\Z)'

        matches = re.findall(pattern, content, re.DOTALL)

        for match in matches:
            story_num, title, priority, description, why_priority, test, scenarios = match

            # Extract individual scenarios
            scenario_pattern = r'\d+\. \*\*Given\*\* (.*?), \*\*When\*\* (.*?), \*\*Then\*\* (.*?)(?=\n\d+\.|\Z)'
            scenario_matches = re.findall(scenario_pattern, scenarios, re.DOTALL)

            scenarios_list = []
            for given, when, then in scenario_matches:
                scenarios_list.append({
                    'given': given.strip(),
                    'when': when.strip(),
                    'then': then.strip()
                })

            stories.append({
                'id': f'US{story_num}',
                'title': title.strip(),
                'priority': priority,
                'description': description.strip(),
                'rationale': why_priority.strip(),
                'test': test.strip(),
                'scenarios': scenarios_list
            })

        return stories

    def extract_functional_requirements(self) -> Dict[str, List[Dict[str, str]]]:
        """Extract functional requirements grouped by category"""
        content = self._load_file(self.spec_file)
        requirements = {}

        # Find functional requirements section
        fr_section = re.search(r'## Functional Requirements(.*?)## Success Criteria', content, re.DOTALL)
        if not fr_section:
            return requirements

        fr_content = fr_section.group(1)

        # Extract categories and their requirements
        category_pattern = r'### (.*?)\n(.*?)(?=###|\Z)'
        category_matches = re.findall(category_pattern, fr_content, re.DOTALL)

        for category, reqs in category_matches:
            req_pattern = r'- \*\*(FR-\d+)\*\*: (.*?)(?=\n- |\Z)'
            req_matches = re.findall(req_pattern, reqs, re.DOTALL)

            requirements[category.strip()] = [
                {
                    'id': req_id,
                    'description': desc.strip()
                }
                for req_id, desc in req_matches
            ]

        return requirements

    def extract_business_rules(self) -> List[Dict[str, Any]]:
        """Extract business rules from spec.md"""
        content = self._load_file(self.spec_file)
        rules = []

        # Find Key Entities section with business rules
        entities_section = re.search(r'## Key Entities and Business Rules(.*?)## Functional Requirements', content, re.DOTALL)
        if not entities_section:
            return rules

        entities_content = entities_section.group(1)

        # Extract entities and their rules
        entity_pattern = r'### \d+\. (.*?)\n(.*?)(?=###|\Z)'
        entity_matches = re.findall(entity_pattern, entities_content, re.DOTALL)

        for entity_name, entity_content in entity_matches:
            # Extract business rules for this entity
            rule_pattern = r'- (.*?)(?=\n- |\Z)'
            rule_matches = re.findall(rule_pattern, entity_content, re.DOTALL)

            for rule in rule_matches:
                if 'Business Rule' in rule or 'must' in rule.lower() or 'should' in rule.lower():
                    rules.append({
                        'entity': entity_name.strip(),
                        'rule': rule.strip()
                    })

        return rules

    def extract_database_entities(self) -> List[Dict[str, Any]]:
        """Extract database entity definitions from data-model.md"""
        if not self.data_model_file.exists():
            # Try to extract from spec.md instead
            return self._extract_entities_from_spec()

        content = self._load_file(self.data_model_file)
        entities = []

        # Pattern to match entity definitions
        pattern = r'## \d+\. (.*?)\n(.*?)(?=## \d+\.|\Z)'
        matches = re.findall(pattern, content, re.DOTALL)

        for entity_name, entity_content in matches:
            # Extract table name
            table_match = re.search(r'Table Name: `(.*?)`', entity_content)
            table_name = table_match.group(1) if table_match else ''

            # Extract description
            desc_match = re.search(r'Description: (.*?)\n', entity_content)
            description = desc_match.group(1) if desc_match else ''

            # Extract fields
            fields = []
            field_pattern = r'- `(.*?)` \((.*?)\): (.*?)(?=\n- |\Z)'
            field_matches = re.findall(field_pattern, entity_content, re.DOTALL)

            for field_name, field_type, field_desc in field_matches:
                fields.append({
                    'name': field_name,
                    'type': field_type,
                    'description': field_desc.strip()
                })

            entities.append({
                'name': entity_name.strip(),
                'table': table_name,
                'description': description.strip(),
                'fields': fields
            })

        return entities

    def _extract_entities_from_spec(self) -> List[Dict[str, Any]]:
        """Fallback: Extract entity information from spec.md"""
        content = self._load_file(self.spec_file)
        entities = []

        # Find Key Entities section
        entities_section = re.search(r'## Key Entities and Business Rules(.*?)## Functional Requirements', content, re.DOTALL)
        if not entities_section:
            return entities

        entities_content = entities_section.group(1)

        # Extract entities
        entity_pattern = r'### \d+\. (.*?) \((.*?)\)(.*?)(?=###|\Z)'
        entity_matches = re.findall(entity_pattern, entities_content, re.DOTALL)

        for entity_name, table_name, entity_content in entity_matches:
            # Extract fields
            fields = []
            field_pattern = r'- (.*?): (.*?)(?=\n- |\Z)'
            field_matches = re.findall(field_pattern, entity_content, re.DOTALL)

            for field_name, field_desc in field_matches:
                if not any(keyword in field_name.lower() for keyword in ['business rule', 'validation', 'constraint']):
                    fields.append({
                        'name': field_name.strip(),
                        'description': field_desc.strip()
                    })

            entities.append({
                'name': entity_name.strip(),
                'table': table_name.strip(),
                'fields': fields
            })

        return entities

    def extract_success_criteria(self) -> List[Dict[str, str]]:
        """Extract success criteria from spec.md"""
        content = self._load_file(self.spec_file)
        criteria = []

        # Find success criteria section
        sc_section = re.search(r'## Success Criteria(.*?)## Assumptions', content, re.DOTALL)
        if not sc_section:
            return criteria

        sc_content = sc_section.group(1)

        # Extract individual criteria
        pattern = r'- \*\*(SC-\d+)\*\*: (.*?)(?=\n- |\Z)'
        matches = re.findall(pattern, sc_content, re.DOTALL)

        for sc_id, description in matches:
            criteria.append({
                'id': sc_id,
                'description': description.strip()
            })

        return criteria

    def extract_assumptions(self) -> List[str]:
        """Extract assumptions from spec.md"""
        content = self._load_file(self.spec_file)
        assumptions = []

        # Find assumptions section
        assumptions_section = re.search(r'## Assumptions(.*?)## Out of Scope', content, re.DOTALL)
        if not assumptions_section:
            return assumptions

        assumptions_content = assumptions_section.group(1)

        # Extract individual assumptions
        pattern = r'- (.*?)(?=\n- |\Z)'
        matches = re.findall(pattern, assumptions_content, re.DOTALL)

        for assumption in matches:
            assumptions.append(assumption.strip())

        return assumptions

    def extract_timeline_phases(self) -> List[Dict[str, Any]]:
        """Extract project phases from plan.md"""
        content = self._load_file(self.plan_file)
        phases = []

        # Find phases section
        phases_section = re.search(r'## Implementation Phases(.*?)(?=## |$)', content, re.DOTALL)
        if not phases_section:
            return phases

        phases_content = phases_section.group(1)

        # Extract individual phases
        pattern = r'### Phase (\d+): (.*?) \((.*?)\)(.*?)(?=### Phase |\Z)'
        matches = re.findall(pattern, phases_content, re.DOTALL)

        for phase_num, title, duration, content in matches:
            # Extract deliverables
            deliverables = []
            deliv_pattern = r'- (.*?)(?=\n- |\Z)'
            deliv_section = re.search(r'\*\*Deliverables\*\*:(.*?)(?=\*\*|$)', content, re.DOTALL)
            if deliv_section:
                deliv_matches = re.findall(deliv_pattern, deliv_section.group(1))
                deliverables = [d.strip() for d in deliv_matches]

            phases.append({
                'id': f'Phase {phase_num}',
                'title': title.strip(),
                'duration': duration.strip(),
                'deliverables': deliverables
            })

        return phases

    def extract_technology_stack(self) -> Dict[str, Any]:
        """Extract technology decisions from research.md"""
        if not self.research_file.exists():
            return self._extract_tech_from_spec()

        content = self._load_file(self.research_file)
        stack = {
            'backend': [],
            'frontend': [],
            'database': [],
            'infrastructure': [],
            'tools': []
        }

        # Extract technology decisions
        tech_section = re.search(r'## Technology Decisions(.*?)(?=## |$)', content, re.DOTALL)
        if tech_section:
            tech_content = tech_section.group(1)

            # Extract individual decisions
            decision_pattern = r'### Decision \d+: (.*?)\n(.*?)(?=### Decision |\Z)'
            decision_matches = re.findall(decision_pattern, tech_content, re.DOTALL)

            for tech_name, tech_desc in decision_matches:
                # Categorize based on keywords
                if any(kw in tech_name.lower() for kw in ['.net', 'api', 'backend']):
                    stack['backend'].append(tech_name.strip())
                elif any(kw in tech_name.lower() for kw in ['react', 'frontend', 'ui']):
                    stack['frontend'].append(tech_name.strip())
                elif any(kw in tech_name.lower() for kw in ['sql', 'database', 'db2']):
                    stack['database'].append(tech_name.strip())
                elif any(kw in tech_name.lower() for kw in ['azure', 'cloud', 'docker']):
                    stack['infrastructure'].append(tech_name.strip())
                else:
                    stack['tools'].append(tech_name.strip())

        return stack

    def _extract_tech_from_spec(self) -> Dict[str, Any]:
        """Fallback: Extract technology stack from spec.md"""
        content = self._load_file(self.spec_file)

        # Default technology stack based on project description
        return {
            'backend': ['.NET 9.0', 'ASP.NET Core', 'Entity Framework Core', 'SoapCore'],
            'frontend': ['React 19', 'TypeScript', 'Vite', 'React Router'],
            'database': ['SQL Server', 'DB2 (legacy)'],
            'infrastructure': ['Azure App Service', 'Azure SQL Database', 'Docker'],
            'tools': ['Visual Studio 2022', 'VS Code', 'Postman', 'PlantUML']
        }

    def extract_component_specifications(self) -> Dict[str, List[Dict[str, str]]]:
        """Extract component specifications from spec.md"""
        content = self._load_file(self.spec_file)
        components = {
            'backend': [],
            'frontend': [],
            'integration': []
        }

        # Extract from functional requirements
        fr = self.extract_functional_requirements()

        # Backend components
        if 'API Endpoints' in fr:
            for req in fr['API Endpoints']:
                components['backend'].append({
                    'name': f"API - {req['id']}",
                    'description': req['description']
                })

        # Frontend components
        if 'User Interface' in fr:
            for req in fr['User Interface']:
                components['frontend'].append({
                    'name': f"UI - {req['id']}",
                    'description': req['description']
                })

        # Integration components
        if 'External Service Integration' in fr:
            for req in fr['External Service Integration']:
                components['integration'].append({
                    'name': f"Integration - {req['id']}",
                    'description': req['description']
                })

        return components

    def extract_all(self) -> Dict[str, Any]:
        """Extract all content and return as dictionary"""
        return {
            'user_stories': self.extract_user_stories(),
            'functional_requirements': self.extract_functional_requirements(),
            'business_rules': self.extract_business_rules(),
            'database_entities': self.extract_database_entities(),
            'success_criteria': self.extract_success_criteria(),
            'assumptions': self.extract_assumptions(),
            'timeline_phases': self.extract_timeline_phases(),
            'technology_stack': self.extract_technology_stack(),
            'component_specifications': self.extract_component_specifications()
        }

    def save_to_json(self, output_path: str):
        """Save extracted content to JSON file"""
        data = self.extract_all()

        output_file = Path(output_path)
        output_file.parent.mkdir(parents=True, exist_ok=True)

        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)

        print(f"Extracted content saved to {output_file}")
        return data


def main():
    """Main function for testing content extraction"""
    import argparse

    parser = argparse.ArgumentParser(description='Extract content from specification files')
    parser.add_argument('--source', '-s',
                       default='../../001-visualage-dotnet-migration',
                       help='Source directory containing spec files')
    parser.add_argument('--output', '-o',
                       default='../output/intermediate/extracted_content.json',
                       help='Output JSON file path')

    args = parser.parse_args()

    # Resolve paths
    script_dir = Path(__file__).parent
    source_dir = (script_dir / args.source).resolve()
    output_file = (script_dir / args.output).resolve()

    # Extract content
    extractor = ContentExtractor(str(source_dir))
    data = extractor.save_to_json(str(output_file))

    # Print summary
    print("\nExtraction Summary:")
    print(f"- User Stories: {len(data['user_stories'])}")
    print(f"- Functional Requirements: {sum(len(v) for v in data['functional_requirements'].values())}")
    print(f"- Business Rules: {len(data['business_rules'])}")
    print(f"- Database Entities: {len(data['database_entities'])}")
    print(f"- Success Criteria: {len(data['success_criteria'])}")
    print(f"- Assumptions: {len(data['assumptions'])}")
    print(f"- Timeline Phases: {len(data['timeline_phases'])}")


if __name__ == '__main__':
    main()