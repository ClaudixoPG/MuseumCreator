# Exploring Procedural Content Generation of Environments for Virtual Museums: a Mixed-Initiative Approach

This project presents a two-phase integrative system for developing virtual museums through digital environment generation. It includes a Room Generator that creates varied spatial layouts using Genetic Algorithms (GAs) and Binary Space Partitioning (BSP) for efficiency and adaptability, and an Artwork Arrangement stage that optimizes artwork placement for space utilization and aesthetic diversity. The system offers a mixed-initiative tool that reduces development time while maintaining creative control, enhancing both the design process and the creation of dynamic, engaging virtual museum environments.

## Getting Started

This project includes a case study of the Virtual Museum of Maule in Chile, which features four distinct collections: Pre-Columbian Cultures, Colony, Crafts, and Rural Life, Chilean Independence, and Pedro Olmos. The collections include 3D pieces digitized through photogrammetry, 3D scanning, and high-definition photography. Users can explore these collections interactively, view 3D objects in 360 degrees, and access cultural information. This system addresses previous limitations in the Virtual Museum of Maule by streamlining the creation of 3D environments and optimizing artifact arrangement, enhancing user experience with dynamic layouts and improved spatial flow.

![Picture1](https://github.com/user-attachments/assets/63b92e90-4370-4431-ba45-1141771e7073)

### Requirements

The system requires use Unity Version 2022.3.52f1 or above

##Stages

### Room Generator

The Room Generator, as the first stage in creating a virtual museum environment, allows users to define spatial layouts by specifying dimensions like width, height, and maximum room size. Using an adaptable Binary Space Partitioning (BSP) algorithm, the system generates various room configurations, offering users flexibility to choose their preferred design. Each layout is translated into a structured model, simplifying future adjustments and assigning basic structure types (e.g., walls, floors, doors). Visual representations assist users in assessing layouts and planning exhibit placements. This mixed-initiative approach empowers users to maintain control over the environment’s design while reducing time-consuming tasks, ensuring that the virtual museum’s layout aligns with the curated narrative and collection.

![step1_v5](https://github.com/user-attachments/assets/ac4e6a5c-e2c3-4397-b76d-78eaa1f1c159)

### Artwork Arrangement

The Artwork Arrangement stage optimizes the placement of artworks within the generated museum layout using a Genetic Algorithm, which evaluates multiple configurations to ensure accessibility and aesthetic balance. This process simplifies spatial organization into a one-dimensional floor mapping, retaining only feasible layouts that meet spatial and accessibility requirements. To further ensure the feasibility of the generated layouts, a path verification module was developed. It constructs a spatial representation capturing connections between rooms and key access points, using the Held-Karp heuristic to calculate optimal internal paths based on exhibit placement and entry points. The verification process includes four sub-stages: (A) identifying room structure and navigable areas, (B) configuring artwork placement, (C) assigning unique codes to rooms, and (D) combining these configurations to verify that the space is fully traversable.

![Frame 3](https://github.com/user-attachments/assets/9374d758-0e65-4100-80af-f45eac54fdaa)

## Example of a generated museum

The proposed system demonstrates potential in generating structured and diverse virtual museum environments through the combination of BSP and GA, enabling varied spatial configurations with logical exhibition organization. While the layouts support different exhibition styles, further evaluation with visitors is needed to assess navigability and user experience. Incorporating additional constraints, such as visitor flow analysis or thematic grouping, could enhance the system’s adaptability, making it a valuable tool for curators and designers.

![Final_Representation](https://github.com/user-attachments/assets/aa99f70c-d005-4257-9b89-9005e7d9bf22)

## Notes

This repository is part of a Research Article titled "Exploring Procedural Content Generation of Environments for Virtual Museums: a Mixed-Initiative Approach", for more details about each stage and the performance analyses performed, download the associated paper (ref to the paper's doi)

## Publications

Aquí debieran ir las publicaciones relacionadas

## Authors

* **Claudio Rubio Naranjo** - [ClaudixoPG](https://github.com/ClaudixoPG); crubio17@alumnos.utalca.cl; claudiorubio23@gmail.com
* **Felipe Besoain** - [Fbesoain](https://github.com/fbesoain); fbesoain@utalca.cl
* **Nicolas A. Barriga** - [Nbarriga](https://github.com/nbarriga); nbarriga@utalca.cl
* **Ben Ingram**
* **Huizilopoztli Luna-García**

## Contributions (CRediT author statement)
 
Conceptualization, C.R. and F.B.; Data curation, C.R.; Formal analysis, C.R.; Funding acquisition, N.A.B.; Investigation, C.R., F.B. and N.A.B.; Methodology, C.R. and F.B.; Project administration, C.R. and F.B; Resources, F.B. and N.A.B;
Software, C.R.; Supervision, F.B. and N.A.B.; Validation, C.R., N.A.B., B.I., H.L. and F.B.; Visualization, C.R.; Writing — original draft, C.R.; Writing — review and editing, F.B., B.I., N.A.B. and H.L. All authors have read and agreed to the published version of the manuscript.

## License

AQUI NO SE QUE PONER

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Research funded by Agencia Nacional de Investigación y Desarrollo, ANID-Subdirección del Capital Humano/Doctorado Nacional/2023-21232404 and FONDECYT Iniciación grant 11220438.
