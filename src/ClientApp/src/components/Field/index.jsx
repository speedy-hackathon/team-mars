import React from "react";
import styles from "./styles.module.css";
import Person from "../Person";
import House from "../House";
import {v4 as uuidv4} from 'uuid';

export default function Field({ map, people, onClick }) {
  return (
    <div className={styles.root}>
      {map.map((item, i) => (
        <House key={uuidv4()} x={item.x} y={item.y} />
      ))}
      {people.map((item) => (
        <Person person={item} key={uuidv4()} onClick={onClick} />
      ))}
    </div>
  );
}
