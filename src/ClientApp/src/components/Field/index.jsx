import React from "react";
import styles from "./styles.module.css";
import Person from "../Person";
import House from "../House";

export default function Field({ map, people, onClick }) {
  return (
    <div className={styles.root}>
      {map.map((item, i) => (
        <House key={item.id} x={item.x} y={item.y} />
      ))}
      {people.map((item, i) => (
        <Person person={item} key={item.id} onClick={onClick} />
      ))}
    </div>
  );
}
