import React, {useEffect} from "react";
import styles from "./styles.module.css";
import Person from "../Person";
import House from "../House";

export default function Field({map, people, onClick, clickedPerson, dictionary}) {
  let height = 0;
  let width = 0;
  useEffect(() => {
    width = document.getElementById("field").clientWidth;
    height = document.getElementById("field").clientHeight;
  }, []);

  if (clickedPerson) {
    const ctx = createContext();
    for (let i = 0; i < dictionary.length - 1; i++) {
      drawLine(ctx, dictionary[i].x, dictionary[i].y, dictionary[i + 1].x, dictionary[i + 1].y);
    }
    clickedPerson = null;
  }

  function createContext() {
    let example = document.getElementById("canvas");
    let ctx = example.getContext('2d');
    example.width = 1034;
    example.height = 522;
    return ctx;
  }

  function drawLine(ctx, x, y, x1, y1) {
    ctx.fillStroke = 'black';
    ctx.beginPath();
    ctx.moveTo(x, y);
    ctx.lineTo(x1, y1);
    ctx.stroke();
  }


  return (
    <div className={styles.root} id="field">
      <canvas id="canvas" className={styles.canvas}>

      </canvas>
      {map.map((item, i) => (
        <House key={item.id} x={item.x} y={item.y} />
      ))}
      {people.map((item) => (
        <Person person={item} key={item.id} onClick={onClick}/>
      ))}
    </div>
  );
}
